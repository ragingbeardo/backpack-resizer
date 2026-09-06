using BackpackResizer.Config;
using SPTarkov.Common.Models.Logging;
using Spectre.Console;

namespace BackpackResizer.Utility;

/// <summary>
/// utility class for prefix and color formatting of log messages
/// </summary>
public class ColorLogger<T>(ISptLogger<T> logger, ModConfig config)
{
    private const string LogModNamePrefix = "Backpack Resizer: ";

    public void Error(string message, Exception? ex = null) =>
        logger.LogWithColor("[ERROR] " + LogModNamePrefix + message, Color.Red, ex: ex);

    public void Warning(string message, Exception? ex = null) =>
        logger.LogWithColor("[WARNING] " + LogModNamePrefix + message, Color.Yellow, ex: ex);

    public void Success(string message, Exception? ex = null) =>
        logger.LogWithColor("[SUCCESS] " + LogModNamePrefix + message, Color.Green, ex: ex);

    public void Info(string message, Exception? ex = null) =>
        logger.LogWithColor("[INFO] " + LogModNamePrefix + message, Color.White, ex: ex);
    
    public void Debug(string message, Exception? ex = null)
    {
        if (!config.DebugLogging)
        {
            return;
        }

        logger.LogWithColor("[DEBUG] " + LogModNamePrefix + message, Color.Grey, ex: ex);
    }
}
