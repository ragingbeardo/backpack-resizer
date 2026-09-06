using System.Text.RegularExpressions;

namespace BackpackResizer.Utility;

/// <summary>
/// utility for identifying identical backpacks with different colors
/// </summary>
public static partial class BackpackColorParser
{
    /// <summary>
    /// splits (color name) from the base name
    /// </summary>
    public static (string BaseName, string? Color) SplitBackpackNameAndColor(string fullName)
    {
        var match = BackpackColorSuffixRegex().Match(fullName);
        return match.Success ? (match.Groups["base"].Value, match.Groups["color"].Value) : (fullName, null);
    }

    /// <summary>
    /// gets the base name to be used for display purposes without the color suffix
    /// </summary>
    public static string GetBaseBackpackName(string anyFullName) =>
        SplitBackpackNameAndColor(anyFullName).BaseName;

    [GeneratedRegex(@"^(?<base>.*?)\s*\((?<color>[^()]+)\)\s*$")]
    private static partial Regex BackpackColorSuffixRegex();
}
