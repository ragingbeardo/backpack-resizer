using BackpackResizer.Config;
using BackpackResizer.Utility;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;

namespace BackpackResizer.Presets;

public enum PresetSource
{
    /// <summary>
    /// ships with the mod, read only
    /// </summary>
    BuiltIn,

    /// <summary>
    /// built by the mod for the current install, read only
    /// </summary>
    Generated,

    /// <summary>
    /// an importable/exportable file source
    /// </summary>
    User,
}

/// <param name="Key">identifies the preset in the UI. For a <see cref="PresetSource.User"/> preset this is its file name</param>
public record PresetEntry(string Key, Preset Preset, PresetSource Source);

[Injectable(InjectionType.Singleton)]
public class PresetRepository(ISptLogger<PresetRepository> logger, ModConfig config)
{
    private readonly ColorLogger<PresetRepository> _log = new(logger, config);

    public IReadOnlyList<PresetEntry> Presets { get; private set; } = [];

    public PresetEntry? Find(string key) => Presets.FirstOrDefault(entry => entry.Key == key);

    /// <summary>
    /// rescans the presets folder so files dropped in while the server is running show up
    /// </summary>
    public async Task ReloadAsync(CancellationToken ct = default)
    {
        var entries = new List<PresetEntry>
        {
            new(ModConfig.OriginalValuesPresetKey, config.OriginalValues, PresetSource.Generated),
        };

        entries.AddRange(DefaultPresets.BuildAll()
            .Select(builtIn => new PresetEntry(builtIn.Key, builtIn.Preset, PresetSource.BuiltIn)));

        var files = await PresetFiles.LoadAllAsync(message => _log.Warning(message), ct);
        entries.AddRange(files
            .OrderBy(file => file.Preset.Name, StringComparer.OrdinalIgnoreCase)
            .Select(file => new PresetEntry(file.FileName, file.Preset, PresetSource.User)));

        Presets = entries;
    }

    /// <summary>
    /// writes the preset to its own file and returns the new preset's key
    /// </summary>
    public async Task<string> CreateAsync(Preset preset, CancellationToken ct = default)
    {
        var fileName = await PresetFiles.WriteAsync(preset, ct);
        await ReloadAsync(ct);
        return fileName;
    }

    /// <summary>
    /// overwrites the preset's file with the given preset. Only user presets can be updated.
    /// </summary>
    public async Task<bool> UpdateAsync(string key, Preset preset, CancellationToken ct = default)
    {
        if (Find(key) is not { Source: PresetSource.User } || !await PresetFiles.OverwriteAsync(key, preset, ct))
        {
            return false;
        }

        await ReloadAsync(ct);
        return true;
    }

    /// <summary>
    /// deletes the preset's file. Only user presets can be deleted.
    /// </summary>
    public async Task<bool> DeleteAsync(string key, CancellationToken ct = default)
    {
        if (Find(key) is not { Source: PresetSource.User } || !PresetFiles.Delete(key))
        {
            return false;
        }

        await ReloadAsync(ct);
        return true;
    }
}
