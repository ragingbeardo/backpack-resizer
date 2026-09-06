namespace BackpackResizer;

using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Web;

// ReSharper disable once ClassNeverInstantiated.Global                                                                                                                                                                                                                                                          
public record ModMetadata : IModMetadata, IModBlazorMetadata
{
    public const string WebRootUrl = "BackpackResizer";

    public string? WWWRootUrl { get; init; } = WebRootUrl;
    public string? HomePage { get; init; } = "/backpackresizer";
    public string? HomePageDescription { get; init; } = "Configure grid size of backpacks.";
    
    public string ModGuid { get; init; } = "io.ragingbeardo.backpackresizer";
    public string Name { get; init; } = "BackpackResizer";
    public string Author { get; init; } = "RagingBeardo";
    public List<string>? Contributors { get; init; }
    public SemanticVersioning.Version Version { get; init; } = new("1.0.0");
    
    public SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.0");
    public List<string>? Incompatibilities { get; init; }
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    
    public string? Url { get; init; } = "https://github.com/ragingbeardo/backpack-resizer";
    public string License { get; init; } = "MIT";
    
    public bool HasPrepatcher { get; init; } = false;
}
