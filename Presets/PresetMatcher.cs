using BackpackResizer.Config;

namespace BackpackResizer.Presets;

/// <summary>
/// translates between the backpacks on this install and the install-independent entries in a preset
/// </summary>
public static class PresetMatcher
{
    public readonly record struct Match(string FamilyKey, PresetBackpack Entry);

    public readonly record struct Result(List<Match> Matched, List<PresetBackpack> Unmatched);

    /// <summary>
    /// Finds the backpack on this install for each preset entry. An entry matches a backpack when
    /// they share any item id, even for backpacks another mod adds. When no id matches, the entry
    /// falls back to the backpack name. If two entries land on the same backpack, backpack size of
    /// the first wins.
    /// </summary>
    public static Result Resolve(Preset preset, IReadOnlyDictionary<string, BackpackConfig> backpacks)
    {
        var familyKeyByItemId = new Dictionary<string, string>(StringComparer.Ordinal);
        var familyKeyByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (familyKey, backpack) in backpacks)
        {
            foreach (var itemId in backpack.ItemIds)
            {
                familyKeyByItemId.TryAdd(itemId, familyKey);
            }

            if (backpack.BackpackName.Length > 0)
            {
                familyKeyByName.TryAdd(backpack.BackpackName, familyKey);
            }
        }

        var matched = new List<Match>();
        var unmatched = new List<PresetBackpack>();
        var claimedFamilyKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var entry in preset.Backpacks)
        {
            var familyKey = entry.ItemIds
                .Select(familyKeyByItemId.GetValueOrDefault)
                .FirstOrDefault(key => key is not null);

            if (familyKey is null && entry.Name.Length > 0)
            {
                familyKey = familyKeyByName.GetValueOrDefault(entry.Name);
            }

            if (familyKey is null)
            {
                unmatched.Add(entry);
                continue;
            }

            if (claimedFamilyKeys.Add(familyKey))
            {
                matched.Add(new Match(familyKey, entry));
            }
        }

        return new Result(matched, unmatched);
    }

    /// <summary>
    /// captures the current size of every resizable backpack as preset entries, carrying the name
    /// and item ids so the result doesn't depend on this install's family keys
    /// </summary>
    public static List<PresetBackpack> Capture(IReadOnlyDictionary<string, BackpackConfig> backpacks) =>
    [
        .. backpacks.Values
            .Where(backpack => backpack.Grid is not null)
            .OrderBy(backpack => backpack.BackpackName, StringComparer.OrdinalIgnoreCase)
            .Select(backpack => new PresetBackpack
            {
                Name = backpack.BackpackName,
                ItemIds = [.. backpack.ItemIds],
                Width = backpack.Grid!.Width,
                Height = backpack.Grid.Height,
            })
    ];

    /// <summary>
    /// the entries for updating an existing preset: the current size of every resizable backpack, plus
    /// the preset's own entries for backpacks this install can't resize or doesn't have
    /// </summary>
    public static List<PresetBackpack> Update(Preset existing, IReadOnlyDictionary<string, BackpackConfig> backpacks)
    {
        var (matched, unmatched) = Resolve(existing, backpacks);
        var kept = matched
            .Where(match => backpacks[match.FamilyKey].Grid is null)
            .Select(match => match.Entry)
            .Concat(unmatched);

        return [.. Capture(backpacks), .. kept];
    }

    /// <summary>
    /// check for existing entry. used for seeding the original values preset
    /// </summary>
    public static bool Covers(Preset preset, BackpackConfig backpack) =>
        preset.Backpacks.Any(entry => entry.ItemIds.Intersect(backpack.ItemIds).Any());
}
