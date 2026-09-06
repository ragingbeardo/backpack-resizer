namespace BackpackResizer.Config;

public record ModConfig
{
    public const string OriginalValuesPresetKey = "original-values";
    
    public bool ModEnabled { get; set; } = true;
    
    /// <summary>
    /// If true, the mod will allow width greater than 6 and height greater than 15
    /// </summary>
    public bool BypassSoftCapSizeLimits { get; set; }
    
    public bool DebugLogging { get; set; }
    
    /// <summary>
    /// dictionary of active backpack configs if enabled
    /// </summary>
    public Dictionary<string, BackpackConfig> Backpacks { get; init; } = new();
    
    /// <summary>
    /// dictionary of preset grid values
    /// </summary>
    public Dictionary<string, Preset> Presets { get; init; } = new();
}

public record BackpackConfig
{
    public string BackpackName { get; init; } = "";

    /// <summary>
    /// list of one or more item ids that represent varying colors of the same backpack
    /// </summary>
    public List<string> ItemIds { get; set; } = [];
    
    /// <summary>
    /// flagging backpacks that can't be resized
    /// </summary>
    public bool IsMultiPocket { get; set; }
    
    public BackpackGrid? Grid { get; set; }
}

public record Preset
{
    public string Name { get; init; } = "";
    
    public Dictionary<string, BackpackGridPreset> Backpacks { get; } = new();
}

public record BackpackGridPreset
{
    public int Width { get; init; }
    
    public int Height { get; init; }
}

public record BackpackGrid
{
    public int Width { get; set; }
    
    public int Height { get; set; }
}
