using System.Text.Json.Serialization;

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
    /// snapshot of every backpack's size the first time it was discovered on this install
    /// </summary>
    public Preset OriginalValues { get; set; } = new() { Name = "Original Values" };

    /// <summary>
    /// old preset format so it can be handled without user having to migrate anything
    /// </summary>
    [JsonPropertyName("Presets")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, LegacyPreset>? LegacyPresets { get; set; }
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
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    public string Name { get; init; } = "";

    public string Author { get; init; } = "";

    public string Description { get; init; } = "";

    public List<PresetBackpack> Backpacks { get; init; } = [];
}

public record PresetBackpack
{
    /// <summary>
    /// base backpack name
    /// </summary>
    public string Name { get; init; } = "";

    public List<string> ItemIds { get; init; } = [];

    public int Width { get; init; }

    public int Height { get; init; }
}

/// <summary>
/// Legacy preset format that was used in the config file before presets were moved to their own folder.
/// </summary>
public record LegacyPreset
{
    public string Name { get; init; } = "";
    
    public Dictionary<string, BackpackGridPreset> Backpacks { get; init; } = new();
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
