using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.DI;

namespace BackpackResizer.Config;

// ReSharper disable once ClassNeverInstantiated.Global
public class BackpackResizerConfigRegistration : IOnDIConstruct
{
    private const string ConfigHeaderComment =
        """
        // ModEnabled    - turn this bad boy on or off
        // BypassSoftCapSizeLimits - bypass suggested size limits
        // DebugLogging  - extra logging if something appears broken
        // Backpacks     - one entry per backpack. color variants grouped together by ID.
        //   DisplayName - base backpack name (e.g. "Gruppa 99 T30 backpack")
        //   ItemIds     - items IDs for the variants of the same backpack
        //   IsMultiPocket - flags multi-pocket backpacks so they aren't edited
        //   Grid        
        //     Width     - intended backpack width by cell count
        //     Height    - intended backpack height by cell count
        // OriginalValues - every backpack's size as first found on this install. Presets you make or
        //                  download are separate files in the presets folder.

        """;

    internal static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static async Task OnDIConstructAsync(IServiceCollection serviceCollection, CancellationToken ct)
    {
        var config = await LoadConfigFromDiskAsync(ct);
        serviceCollection.AddSingleton(config);
    }

    private static async Task<ModConfig> LoadConfigFromDiskAsync(CancellationToken ct)
    {
        var configPath = GetConfigPath();

        if (!File.Exists(configPath))
        {
            var defaultConfig = new ModConfig();
            await SaveConfigToDiskAsync(defaultConfig, ct);
            return defaultConfig;
        }

        await using var stream = File.OpenRead(configPath);
        var config = await JsonSerializer.DeserializeAsync<ModConfig>(stream, SerializerOptions, ct);
        return config ?? new ModConfig();
    }

    public static async Task SaveConfigToDiskAsync(ModConfig config, CancellationToken ct)
    {
        var configPath = GetConfigPath();
        Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);

        var json = JsonSerializer.Serialize(config, SerializerOptions);

        await using var writer = new StreamWriter(File.Create(configPath));
        await writer.WriteAsync(ConfigHeaderComment);
        await writer.WriteAsync(json);
        await writer.WriteLineAsync();
    }
    
    internal static string GetModDirectory() =>
        Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
        ?? throw new InvalidOperationException("Could not resolve the mod's own directory.");

    private static string GetConfigPath() => Path.Combine(GetModDirectory(), "config.jsonc");
}
