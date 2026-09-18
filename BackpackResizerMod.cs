using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Tables;
using BackpackResizer.Config;
using BackpackResizer.Utility;
using Preset = BackpackResizer.Config.Preset;

namespace BackpackResizer;

[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.PostLoad + 1)]
public class BackpackResizerMod(
    ISptLogger<BackpackResizerMod> logger,
    TemplateTable templateTable,
    ItemHelper itemHelper,
    ModConfig config) : IOnLoad
{
    private readonly ColorLogger<BackpackResizerMod> _log = new(logger, config);
    
    private readonly Dictionary<string, VanillaGridTemplate> _vanillaGridTemplateByFamilyKey = new();

    private readonly record struct VanillaGridTemplate(string GridKey, List<GridFilter> Filters, string? Prototype);

    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        await MergeDuplicateNamedFamiliesAsync(cancellationToken);

        var discoveredFamilies = DiscoverBackpacks();
        var addedCount = SeedDefaults(discoveredFamilies);
        if (addedCount > 0)
        {
            await BackpackResizerConfigRegistration.SaveConfigToDiskAsync(config, cancellationToken);
            _log.Info($"found {addedCount} new backpack design(s) and the config has been updated");
        }

        ResizeAllBackpacks();
    }

    // collapse duplicate backpack entries from before grouping removed 'by grid' in favor of just the name itself
    private async Task MergeDuplicateNamedFamiliesAsync(CancellationToken cancellationToken)
    {
        var duplicateGroups = config.Backpacks
            .GroupBy(kvp => kvp.Value.BackpackName)
            .Where(g => g.Count() > 1)
            .ToList();

        var mergedBackpackNames = new List<string>();

        foreach (var group in duplicateGroups)
        {
            var survivingBackpackFamily = group.OrderBy(kvp => kvp.Key, StringComparer.Ordinal).First();

            foreach (var duplicate in group.Where(kvp => kvp.Key != survivingBackpackFamily.Key))
            {
                survivingBackpackFamily.Value.ItemIds =
                [
                    .. survivingBackpackFamily.Value.ItemIds
                        .Concat(duplicate.Value.ItemIds)
                        .Distinct()
                        .OrderBy(id => id, StringComparer.Ordinal)
                ];

                config.Backpacks.Remove(duplicate.Key);
            }

            survivingBackpackFamily.Value.Grid = null;
            mergedBackpackNames.Add(survivingBackpackFamily.Value.BackpackName);
        }

        if (mergedBackpackNames.Count > 0)
        {
            await BackpackResizerConfigRegistration.SaveConfigToDiskAsync(config, cancellationToken);
            _log.Warning($"duplicate entries were found and merged for: {string.Join(", ", mergedBackpackNames)}. Please reload and save any preset you were using.");
        }
    }

    private readonly record struct Backpack(MongoId Id, string FullName, List<Grid> Grids);

    private readonly record struct DiscoveredBackpackFamily(string FamilyKey, List<Backpack> Group);
    
    private List<DiscoveredBackpackFamily> DiscoverBackpacks()
    {
        var backpacks = new List<Backpack>();

        foreach (var (itemId, item) in templateTable.Items)
        {
            if (!itemHelper.IsOfBaseclass(itemId, BaseClasses.BACKPACK))
            {
                continue;
            }

            var grids = item.Properties?.Grids?.ToList();
            if (grids is null || grids.Count == 0)
            {
                continue;
            }
            
            var fullName = itemHelper.GetItemName(itemId) is { Length: > 0 } localizedName ? localizedName : item.Name ?? itemId.ToString();
            backpacks.Add(new Backpack(itemId, fullName, grids));
        }

        // group backpacks that are the same base but different color
        var groupedBackpacks = backpacks
            .GroupBy(c => BackpackColorParser.SplitBackpackNameAndColor(c.FullName).BaseName)
            .Select(g => g.ToList())
            .ToList();
        
        var existingFamilyKeyByItemId = config.Backpacks
            .SelectMany(kvp => kvp.Value.ItemIds.Select(id => (id, key: kvp.Key)))
            .ToDictionary(t => t.id, t => t.key);

        var discoveredFamilies = new List<DiscoveredBackpackFamily>();

        foreach (var group in groupedBackpacks)
        {
            var itemIds = group.Select(c => c.Id.ToString()).OrderBy(id => id, StringComparer.Ordinal).ToList();
            
            var familyKey = itemIds
                .Select(existingFamilyKeyByItemId.GetValueOrDefault)
                .FirstOrDefault(key => key is not null)
                ?? itemIds[0];

            if (!config.Backpacks.TryGetValue(familyKey, out var backpackOverride))
            {
                backpackOverride = new BackpackConfig { BackpackName = BackpackColorParser.GetBaseBackpackName(group[0].FullName) };
                config.Backpacks[familyKey] = backpackOverride;
            }

            backpackOverride.ItemIds = itemIds;
            backpackOverride.IsMultiPocket = group[0].Grids.Count > 1;

            _log.Debug($"'{backpackOverride.BackpackName}' has {group.Count} variant(s): {string.Join(", ", group.Select(b => $"{b.FullName} [{b.Id}]"))}");

            discoveredFamilies.Add(new DiscoveredBackpackFamily(familyKey, group));
        }

        return discoveredFamilies;
    }
    
    private int SeedDefaults(List<DiscoveredBackpackFamily> discoveredFamilies)
    {
        if (!config.Presets.TryGetValue(ModConfig.OriginalValuesPresetKey, out var originalValuesPreset))
        {
            originalValuesPreset = new Preset { Name = "Original Values" };
            config.Presets[ModConfig.OriginalValuesPresetKey] = originalValuesPreset;
        }

        var addedCount = 0;

        foreach (var (familyKey, group) in discoveredFamilies)
        {
            var backpackOverride = config.Backpacks[familyKey];

            if (backpackOverride.IsMultiPocket || group[0].Grids[0] is not { Name: { } gridKey, Properties: { } gridProperties })
            {
                // A multi-grid design is never resized/restructured, so it's never given a Grid at
                // all (see BackpackOverride.Grid) - nothing here to seed or cache.
                continue;
            }

            // Rebuilt every run regardless of whether Grid below is already seeded - this is the
            // only place vanilla filters/prototype are ever captured, so it must always run before
            // the first ApplyBackpackGrid of this server session.
            _vanillaGridTemplateByFamilyKey[familyKey] = new VanillaGridTemplate(gridKey, gridProperties.Filters?.ToList() ?? [], group[0].Grids[0].Prototype);

            var width = gridProperties.CellsH ?? 0;
            var height = gridProperties.CellsV ?? 0;

            if (backpackOverride.Grid is null)
            {
                backpackOverride.Grid = new BackpackGrid { Width = width, Height = height };
                addedCount++;
            }

            // A snapshot, taken once, of this grid's size the first time it's ever seen - never
            // touched again even if Grid.Width/Height above are later edited by the user.
            originalValuesPreset.Backpacks.TryAdd(familyKey, new BackpackGridPreset { Width = width, Height = height });
        }

        return addedCount;
    }
    
    public void ResizeAllBackpacks()
    {
        if (!config.ModEnabled)
        {
            _log.Info("disabled in config.jsonc, no backpacks were resized.");
            return;
        }

        var resizedCount = 0;

        foreach (var (familyKey, backpackOverride) in config.Backpacks)
        {
            if (backpackOverride.IsMultiPocket)
            {
                // Always the full current variant list, even though nothing here is ever touched -
                // so debug mode shows this design exists and why it's inert, not just silence.
                _log.Debug($"'{backpackOverride.BackpackName}' has {backpackOverride.ItemIds.Count} variant(s) [{string.Join(", ", backpackOverride.ItemIds)}], all left at vanilla size (multi-grid design).");
                continue;
            }

            if (!_vanillaGridTemplateByFamilyKey.TryGetValue(familyKey, out var vanillaGridTemplate))
            {
                _log.Warning($"'{backpackOverride.BackpackName}' has no discovered grid template (its items may no longer exist), skipping it.");
                continue;
            }

            var backpackResizedCount = 0;

            foreach (var itemIdStr in backpackOverride.ItemIds)
            {
                if (!MongoId.IsValidMongoId(itemIdStr) || !templateTable.Items.TryGetValue(new MongoId(itemIdStr), out var item))
                {
                    _log.Warning($"'{backpackOverride.BackpackName}' references unknown item {itemIdStr}, skipping it.");
                    continue;
                }

                ApplyBackpackGrid(item, itemIdStr, backpackOverride, vanillaGridTemplate, ref backpackResizedCount);
            }

            // Always the full current variant list and this run's full result for it - not just
            // what changed - so it's accurate even after a log wipe or right after enabling debug.
            _log.Debug($"'{backpackOverride.BackpackName}' has {backpackOverride.ItemIds.Count} variant(s) [{string.Join(", ", backpackOverride.ItemIds)}]: resized {backpackResizedCount} grid(s).");

            resizedCount += backpackResizedCount;
        }

        _log.Success($"applied {resizedCount} storage grid(s).");
    }
    
    private void ApplyBackpackGrid(TemplateItem item, string itemIdStr, BackpackConfig backpackConfig, VanillaGridTemplate vanillaGridTemplate, ref int resizedCount)
    {
        var gridOverride = backpackConfig.Grid;
        if (gridOverride is null)
        {
            _log.Warning($"'{backpackConfig.BackpackName}' has no configured grid, skipping it.");
            return;
        }

        if (gridOverride.Width <= 0 || gridOverride.Height <= 0)
        {
            _log.Warning($"'{backpackConfig.BackpackName}' has an invalid size ({gridOverride.Width}x{gridOverride.Height}), skipping it.");
            return;
        }

        item.Properties!.Grids = [BuildGrid(itemIdStr, vanillaGridTemplate, gridOverride.Width, gridOverride.Height)];
        resizedCount++;
    }
    
    private static Grid BuildGrid(string itemIdStr, VanillaGridTemplate vanillaGridTemplate, int width, int height)
    {
        return new Grid
        {
            Id = $"{itemIdStr}_{vanillaGridTemplate.GridKey}",
            Name = vanillaGridTemplate.GridKey,
            Parent = itemIdStr,
            Prototype = vanillaGridTemplate.Prototype,
            Properties = new GridProperties
            {
                CellsH = width,
                CellsV = height,
                Filters = vanillaGridTemplate.Filters,
                IsSortingTable = false,
                MinCount = 0,
                MaxCount = 0,
                MaxWeight = 0,
            },
        };
    }
}
