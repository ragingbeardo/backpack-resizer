using BackpackResizer.Utility;

namespace BackpackResizer.Config;

/// <summary>
/// presets that ship with the mod. To add another, give it a key constant and a Build method, then
/// list it in <see cref="BuildAll"/>
/// </summary>
public static class DefaultPresets
{
    private const string BetterBackpacksPresetKey = "better-backpacks";

    private readonly record struct Entry(string ItemId, string FullName, int Width, int Height);

    /// <summary>
    /// builds every built in preset, keyed by the key the UI identifies it with
    /// </summary>
    public static IEnumerable<(string Key, Preset Preset)> BuildAll()
    {
        // yield return (OtherPresetKey, BuildOtherPreset()); -- just add more of these for each preset as needed
        yield return (BetterBackpacksPresetKey, BuildBetterBackpacksPreset());
    }

    /// <summary>
    /// Builds a preset that follows the sizes from JoshMate's Better Backpacks mod with
    /// WTT Content Backport backpacks included.
    /// </summary>
    private static Preset BuildBetterBackpacksPreset() => Build(
        name: "Better Backpacks",
        author: "RagingBeardo",
        description: "Sizes from JoshMate's Better Backpacks mod. WTT Content Backport backpacks included in preset.",
        entries:
        [
            new Entry("544a5cde4bdc2d39388b456b", "Flyye MBSS backpack (UCP)", 5, 6),
            new Entry("545cdae64bdc2d39198b4568", "Camelbak Tri-Zip assault backpack (Foliage / MultiCam)", 6, 7),
            new Entry("56e335e4d2720b6c058b456d", "Scav backpack", 6, 6),
            new Entry("56e33634d2720bd8058b456b", "Duffle bag", 6, 4),
            new Entry("56e33680d2720be2748b4576", "Transformer Bag", 5, 4),
            new Entry("59e763f286f7742ee57895da", "Pilgrim tourist backpack", 6, 10),
            new Entry("5ab8ebf186f7742d8b372e80", "SSO Attack 2 raid backpack (Khaki)", 6, 10),
            new Entry("5ab8ee7786f7742d8f33f0b9", "VKBO army bag", 4, 5),
            new Entry("5ab8f04f86f774585f4237d8", "Tactical sling bag (Khaki)", 4, 4),
            new Entry("5b44c6ae86f7742d1627baea", "ANA Tactical Beta 2 Battle backpack (Olive Drab)", 6, 7),
            new Entry("5c0e774286f77468413cc5b2", "Mystery Ranch Blackjack 50 backpack (MultiCam)", 6, 11),
            new Entry("5ca20d5986f774331e7c9602", "WARTECH Berkut BB-102 backpack (A-TACS FG)", 6, 6),
            new Entry("5df8a4d786f77412672a1e3b", "6Sh118 raid backpack (EMR)", 6, 13),
            new Entry("5e4abc6786f77406812bd572", "LBT-2670 Slim Field Med Pack (Black)", 6, 10),
            new Entry("5e997f0b86f7741ac73993e2", "Sanitar's bag", 6, 7),
            new Entry("5e9dcf5986f7746c417435b3", "LBT-8005A Day Pack backpack (MultiCam Black)", 6, 6),
            new Entry("5f5e45cc5021ce62144be7aa", "LolKek 3F Transfer tourist backpack", 4, 6),
            new Entry("5f5e467b0bc58666c37e7821", "Eberlestock F5 Switchblade backpack (Dry Earth)", 6, 7),
            new Entry("6034d103ca006d2dca39b3f0", "Hazard 4 Takedown sling backpack (Black / MultiCam)", 5, 9),
            new Entry("60a272cc93ef783291411d8e", "Hazard 4 Drawbridge backpack (Coyote Tan)", 6, 6),
            new Entry("60a2828e8689911a226117f9", "Hazard 4 Pillbox backpack (Black)", 6, 6),
            new Entry("618bb76513f5097c8d5aa2d5", "Gruppa 99 T20 backpack (Umber Brown / MultiCam)", 6, 6),
            new Entry("618cfae774bb2d036a049e7c", "LBT-1476A 3Day Pack (Woodland / MultiCam Alpine)", 6, 6),
            new Entry("61b9e1aaef9a1b5d6a79899a", "Santa's bag", 6, 10),
            new Entry("628bc7fb408e2b2e9c0801b1", "Mystery Ranch NICE COMM 3 BVS frame system (Coyote)", 6, 12),
            new Entry("628e1ffc83ec92260c0f437f", "Gruppa 99 T30 backpack (Black / MultiCam)", 6, 7),
            new Entry("639346cc1c8f182ad90c8972", "Tasmanian Tiger Trooper 35 backpack (Khaki)", 6, 10),
            new Entry("66a9f98f3bd5a41b162030f4", "Partisan's bag", 6, 8),
            new Entry("66b5f247af44ca0014063c02", "Vertx Ready Pack backpack (Red)", 6, 6),

            // The following aren't vanilla - they're added by the WTT-ContentBackport mod.
            // Mystery Ranch 2 Day Assault Pack (Black) - vanilla 5x6
            new Entry("68947ab5a733b1602007e2fe", "Mystery Ranch 2 Day Assault Pack (Black)", 6, 7),
            // Tasmanian Tiger Modular Pack 45 Plus (MultiCam Black) - vanilla 5x8
            new Entry("68947a8ce4bf255d1b0ca759", "Tasmanian Tiger Modular Pack 45 Plus (MultiCam Black)", 6, 10),
            // Mystery Ranch NICE Frame Load Sling - vanilla 5x7
            new Entry("68947ad3e4bf255d1b0ca75c", "Mystery Ranch NICE Frame Load Sling", 6, 10),
        ]);

    private static Preset Build(string name, string author, string description, IEnumerable<Entry> entries) => new()
    {
        Name = name,
        Author = author,
        Description = description,
        Backpacks =
        [
            .. entries.Select(entry => new PresetBackpack
            {
                Name = BackpackColorParser.GetBaseBackpackName(entry.FullName),
                ItemIds = [entry.ItemId],
                Width = entry.Width,
                Height = entry.Height,
            }),
        ],
    };
}
