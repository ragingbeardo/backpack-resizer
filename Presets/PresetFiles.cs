using System.Text.Json;
using System.Text.RegularExpressions;
using BackpackResizer.Config;

namespace BackpackResizer.Presets;

public static partial class PresetFiles
{
    private const string FileExtension = ".json";
    private const long MaxFileBytes = 256 * 1024;
    private const int MaxSlugLength = 60;

    public readonly record struct LoadedFile(string FileName, Preset Preset);

    public static string DirectoryPath { get; } = Path.Combine(BackpackResizerConfigRegistration.GetModDirectory(), "presets");

    /// <summary>
    /// loads every readable preset file. unreadable or oversized files are reported and skipped.
    /// </summary>
    public static async Task<List<LoadedFile>> LoadAllAsync(Action<string> warn, CancellationToken ct)
    {
        var loaded = new List<LoadedFile>();

        if (!Directory.Exists(DirectoryPath))
        {
            return loaded;
        }

        foreach (var path in Directory.EnumerateFiles(DirectoryPath, "*" + FileExtension).Order(StringComparer.OrdinalIgnoreCase))
        {
            var fileName = Path.GetFileName(path);

            try
            {
                if (new FileInfo(path).Length > MaxFileBytes)
                {
                    warn($"presets/{fileName} is larger than {MaxFileBytes / 1024} KB, skipping it.");
                    continue;
                }

                await using var stream = File.OpenRead(path);
                var preset = await JsonSerializer.DeserializeAsync<Preset>(stream, BackpackResizerConfigRegistration.SerializerOptions, ct);
                if (preset is null)
                {
                    warn($"presets/{fileName} is empty, skipping it.");
                    continue;
                }

                loaded.Add(new LoadedFile(fileName, Sanitize(preset, fileName, warn)));
            }
            catch (JsonException ex)
            {
                warn($"presets/{fileName} isn't a valid preset file, skipping it: {ex.Message}");
            }
            catch (IOException ex)
            {
                warn($"presets/{fileName} couldn't be read, skipping it: {ex.Message}");
            }
        }

        return loaded;
    }
    
    public static async Task<string> WriteAsync(Preset preset, CancellationToken ct)
    {
        Directory.CreateDirectory(DirectoryPath);

        var slug = Slugify(preset.Name);
        var fileName = slug + FileExtension;
        for (var suffix = 2; File.Exists(Path.Combine(DirectoryPath, fileName)); suffix++)
        {
            fileName = $"{slug}-{suffix}{FileExtension}";
        }

        var json = JsonSerializer.Serialize(preset, BackpackResizerConfigRegistration.SerializerOptions);
        await File.WriteAllTextAsync(Path.Combine(DirectoryPath, fileName), json + Environment.NewLine, ct);
        return fileName;
    }
    
    public static bool Delete(string fileName)
    {
        if (Path.GetFileName(fileName) != fileName || !fileName.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var path = Path.Combine(DirectoryPath, fileName);
        if (!File.Exists(path))
        {
            return false;
        }

        File.Delete(path);
        return true;
    }

    /// <summary>
    /// drops anything unusable from a preset that came from a file. Entries with a non-positive
    /// size or no way to identify the backpack are removed instead of being applied.
    /// </summary>
    private static Preset Sanitize(Preset preset, string fileName, Action<string> warn)
    {
        if (preset.SchemaVersion > Preset.CurrentSchemaVersion)
        {
            warn($"presets/{fileName} was made by a newer version of this mod, some of it may be ignored.");
        }

        var entries = new List<PresetBackpack>();
        var skippedCount = 0;

        foreach (var entry in preset.Backpacks ?? [])
        {
            var name = (entry.Name ?? "").Trim();
            var itemIds = (entry.ItemIds ?? [])
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct()
                .ToList();

            if (entry.Width <= 0 || entry.Height <= 0 || (itemIds.Count == 0 && name.Length == 0))
            {
                skippedCount++;
                continue;
            }

            entries.Add(entry with { Name = name, ItemIds = itemIds });
        }

        if (skippedCount > 0)
        {
            warn($"presets/{fileName} had {skippedCount} entr{(skippedCount == 1 ? "y" : "ies")} with an invalid size or no backpack identity, skipped.");
        }

        var presetName = (preset.Name ?? "").Trim();

        return preset with
        {
            Name = presetName.Length > 0 ? presetName : Path.GetFileNameWithoutExtension(fileName),
            Author = (preset.Author ?? "").Trim(),
            Description = (preset.Description ?? "").Trim(),
            Backpacks = entries,
        };
    }

    private static string Slugify(string name)
    {
        var slug = NonSlugCharactersRegex().Replace(name.ToLowerInvariant(), "-").Trim('-');
        if (slug.Length > MaxSlugLength)
        {
            slug = slug[..MaxSlugLength].TrimEnd('-');
        }

        if (slug.Length == 0)
        {
            return "preset";
        }
        
        return ReservedWindowsNameRegex().IsMatch(slug) ? slug + "-preset" : slug;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugCharactersRegex();

    [GeneratedRegex(@"^(con|prn|aux|nul|com\d|lpt\d)$")]
    private static partial Regex ReservedWindowsNameRegex();
}
