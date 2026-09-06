namespace BackpackResizer.Config;

public static class DefaultPresets
{
    public const string BetterBackpacksPresetKey = "better-backpacks";
    
    /// <summary>
    /// Builds a preset that follows the sizes from JoshMate's Better Backpacks mod with
    /// WTT Content Backport backpacks included.
    /// </summary>
    public static Preset BuildBetterBackpacksPreset()
    {
        var preset = new Preset { Name = "Better Backpacks" };

        void Add(string familyKey, int width, int height) =>
            preset.Backpacks[familyKey] = new BackpackGridPreset { Width = width, Height = height };

        // Flyye MBSS backpack (UCP)
        Add("544a5cde4bdc2d39388b456b", 5, 6);
        // Camelbak Tri-Zip assault backpack (Foliage / MultiCam)
        Add("545cdae64bdc2d39198b4568", 6, 7);
        // Scav backpack
        Add("56e335e4d2720b6c058b456d", 6, 6);
        // Duffle bag
        Add("56e33634d2720bd8058b456b", 6, 4);
        // Transformer Bag
        Add("56e33680d2720be2748b4576", 5, 4);
        // Pilgrim tourist backpack
        Add("59e763f286f7742ee57895da", 6, 10);
        // SSO Attack 2 raid backpack (Khaki)
        Add("5ab8ebf186f7742d8b372e80", 6, 10);
        // VKBO army bag
        Add("5ab8ee7786f7742d8f33f0b9", 4, 5);
        // Tactical sling bag (Khaki)
        Add("5ab8f04f86f774585f4237d8", 4, 4);
        // ANA Tactical Beta 2 Battle backpack (Olive Drab)
        Add("5b44c6ae86f7742d1627baea", 6, 7);
        // Mystery Ranch Blackjack 50 backpack (MultiCam)
        Add("5c0e774286f77468413cc5b2", 6, 11);
        // WARTECH Berkut BB-102 backpack (A-TACS FG)
        Add("5ca20d5986f774331e7c9602", 6, 6);
        // 6Sh118 raid backpack (EMR)
        Add("5df8a4d786f77412672a1e3b", 6, 13);
        // LBT-2670 Slim Field Med Pack (Black)
        Add("5e4abc6786f77406812bd572", 6, 10);
        // Sanitar's bag
        Add("5e997f0b86f7741ac73993e2", 6, 7);
        // LBT-8005A Day Pack backpack (MultiCam Black)
        Add("5e9dcf5986f7746c417435b3", 6, 6);
        // LolKek 3F Transfer tourist backpack
        Add("5f5e45cc5021ce62144be7aa", 4, 6);
        // Eberlestock F5 Switchblade backpack (Dry Earth)
        Add("5f5e467b0bc58666c37e7821", 6, 7);
        // Hazard 4 Takedown sling backpack (Black / MultiCam)
        Add("6034d103ca006d2dca39b3f0", 5, 9);
        // Hazard 4 Drawbridge backpack (Coyote Tan)
        Add("60a272cc93ef783291411d8e", 6, 6);
        // Hazard 4 Pillbox backpack (Black)
        Add("60a2828e8689911a226117f9", 6, 6);
        // Gruppa 99 T20 backpack (Umber Brown / MultiCam)
        Add("618bb76513f5097c8d5aa2d5", 6, 6);
        // LBT-1476A 3Day Pack (Woodland / MultiCam Alpine)
        Add("618cfae774bb2d036a049e7c", 6, 6);
        // Santa's bag
        Add("61b9e1aaef9a1b5d6a79899a", 6, 10);
        // Mystery Ranch NICE COMM 3 BVS frame system (Coyote)
        Add("628bc7fb408e2b2e9c0801b1", 6, 12);
        // Gruppa 99 T30 backpack (Black / MultiCam)
        Add("628e1ffc83ec92260c0f437f", 6, 7);
        // Tasmanian Tiger Trooper 35 backpack (Khaki)
        Add("639346cc1c8f182ad90c8972", 6, 10);
        // Partisan's bag
        Add("66a9f98f3bd5a41b162030f4", 6, 8);
        // Vertx Ready Pack backpack (Red)
        Add("66b5f247af44ca0014063c02", 6, 6);

        // The following aren't vanilla - they're added by the WTT-ContentBackport mod. 
        // Mystery Ranch 2 Day Assault Pack (Black) - vanilla 5x6
        Add("68947ab5a733b1602007e2fe", 6, 7);
        // Tasmanian Tiger Modular Pack 45 Plus (MultiCam Black) - vanilla 5x8
        Add("68947a8ce4bf255d1b0ca759", 6, 10);
        // Mystery Ranch NICE Frame Load Sling - vanilla 5x7
        Add("68947ad3e4bf255d1b0ca75c", 6, 10);

        return preset;
    }
}
