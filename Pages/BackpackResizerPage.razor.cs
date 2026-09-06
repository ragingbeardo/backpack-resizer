using System.Text.Json;
using Microsoft.JSInterop;
using BackpackResizer.Config;

namespace BackpackResizer.Pages;

public partial class BackpackResizerPage
{
    private static readonly JsonSerializerOptions SnapshotOptions = new() { IncludeFields = true };

    /// <summary>
    /// soft cap on how wide a backpack can be resized to. overrideable by the user
    /// </summary>
    private const int SoftWidthLimit = 6;

    /// <summary>
    /// soft cap on how tall a backpack can be resized to. overrideable by the user
    /// </summary>
    private const int SoftHeightLimit = 15;

    private string _filterText = "";
    private string? _statusMessage;
    private readonly HashSet<string> _expandedBackpacks = new();

    /// <summary>
    /// snapshot of the save state to detect unsaved changes
    /// </summary>
    private string? _savedSnapshot;

    protected override void OnInitialized()
    {
        _savedSnapshot = Snapshot();
    }

    private string Snapshot() => JsonSerializer.Serialize(Config, SnapshotOptions);

    private bool HasUnsavedChanges => Snapshot() != _savedSnapshot;

    private bool? _isBrowserAwareOfUnsavedChanges;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var isPageDirty = HasUnsavedChanges;
        if (_isBrowserAwareOfUnsavedChanges == isPageDirty)
        {
            return;
        }

        _isBrowserAwareOfUnsavedChanges = isPageDirty;
        await JS.InvokeVoidAsync("backpackResizer.setDirty", isPageDirty);
    }

    private IEnumerable<BackpackConfig> FilteredBackpacks()
    {
        var ordered = Config.Backpacks.Values.OrderBy(b => b.BackpackName);
        if (string.IsNullOrWhiteSpace(_filterText))
        {
            return ordered;
        }

        return ordered.Where(b => b.BackpackName.Contains(_filterText, StringComparison.OrdinalIgnoreCase));
    }

    // Backpacks aren't keyed by anything on BackpackOverride itself, so look the dictionary key
    // back up by reference - cheap enough for a list this size and avoids threading the id through
    // every render.
    private string BackpackDictionaryKey(BackpackConfig backpack) =>
        Config.Backpacks.First(kv => ReferenceEquals(kv.Value, backpack)).Key;

    private void ToggleExpanded(string key)
    {
        if (!_expandedBackpacks.Add(key))
        {
            _expandedBackpacks.Remove(key);
        }
    }

    private async Task SaveAsync()
    {
        await BackpackResizerConfigRegistration.SaveConfigToDiskAsync(Config, CancellationToken.None);
        ResizerMod.ResizeAllBackpacks();
        _savedSnapshot = Snapshot();
        _statusMessage = $"Saved and applied at {DateTime.Now:T}.";
    }
}
