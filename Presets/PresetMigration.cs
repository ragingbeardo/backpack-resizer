using BackpackResizer.Config;
using BackpackResizer.Utility;

namespace BackpackResizer.Presets;

/// <summary>
/// one-time move of presets out of config.jsonc
/// </summary>
public static class PresetMigration
{
    public static async Task MigrateAsync<T>(ModConfig config, ColorLogger<T> log, CancellationToken ct)
    {
        if (config.LegacyPresets is not { } legacyPresets)
        {
            return;
        }

        var builtIns = DefaultPresets.BuildAll().ToDictionary(builtIn => builtIn.Key, builtIn => builtIn.Preset);
        var notMigrated = new Dictionary<string, LegacyPreset>();

        foreach (var (key, legacyPreset) in legacyPresets)
        {
            var converted = Convert(key, legacyPreset, config);

            if (key == ModConfig.OriginalValuesPresetKey)
            {
                config.OriginalValues = converted with { Name = "Original Values" };
                continue;
            }

            // if the preset is identical to a built-in preset, no need for any migration
            if (builtIns.TryGetValue(key, out var builtIn) && HasSameSizes(converted, builtIn))
            {
                continue;
            }

            try
            {
                var fileName = await PresetFiles.WriteAsync(converted, ct);
                log.Info($"moved preset '{converted.Name}' out of config.jsonc into presets/{fileName}");
            }
            catch (IOException ex)
            {
                log.Error($"couldn't move preset '{converted.Name}' into the presets folder, it stays in config.jsonc for now.", ex);
                notMigrated[key] = legacyPreset;
            }
        }

        config.LegacyPresets = notMigrated.Count > 0 ? notMigrated : null;
        await BackpackResizerConfigRegistration.SaveConfigToDiskAsync(config, ct);
    }

    private static Preset Convert(string presetKey, LegacyPreset legacyPreset, ModConfig config) => new()
    {
        Name = string.IsNullOrWhiteSpace(legacyPreset.Name) ? presetKey : legacyPreset.Name,
        Backpacks =
        [
            .. legacyPreset.Backpacks.Select(kvp =>
            {
                config.Backpacks.TryGetValue(kvp.Key, out var backpack);

                return new PresetBackpack
                {
                    Name = backpack?.BackpackName ?? "",
                    // the old key was itself one of the family's item ids
                    ItemIds = [.. (backpack?.ItemIds ?? []).Append(kvp.Key).Distinct()],
                    Width = kvp.Value.Width,
                    Height = kvp.Value.Height,
                };
            })
        ],
    };

    private static bool HasSameSizes(Preset candidate, Preset builtIn) =>
        candidate.Backpacks.Count == builtIn.Backpacks.Count
        && candidate.Backpacks.All(entry => builtIn.Backpacks.Any(builtInEntry =>
            builtInEntry.ItemIds.Intersect(entry.ItemIds).Any()
            && builtInEntry.Width == entry.Width
            && builtInEntry.Height == entry.Height));
}
