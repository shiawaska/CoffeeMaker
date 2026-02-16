using System.Runtime.InteropServices;
using StartupScriptApp.Enums;
using StartupScriptApp.Interfaces;
using StartupScriptApp.Models;

namespace StartupScriptApp.Services;

public class Logging : ILogger
{
    /// <summary>
    ///  Retrieves the last-error code set by the thread. The Last-Error code is maintained on a per-thread basis. Multiple threads do not overwrite each other's last-error code.'
    /// </summary>
    /// <returns>  The last-error code set by the thread. </returns>
    [DllImport("kernal32.dll")]
    private static extern uint GetLastError();

    /// <summary>
    ///  Formats a message string. The message can contain insertion sequences that cause additional information to be inserted into the string.
    /// </summary>
    /// <param name="dwFlags"> Flags that specify the formatting options. </param>
    /// <param name="lpSource"> A pointer to the message source. </param>
    /// <param name="dwMessageId"> The message identifier. </param>
    /// <param name="dwLanguageId"> The language identifier. </param>
    /// <param name="lpBuffer"> A pointer to the buffer that receives the formatted message string. </param>
    /// <param name="nSize"> The size of the buffer, in characters. </param>
    /// <param name="arguments"> A list of pointers to the arguments to be inserted into the message string. </param>
    /// <returns> The number of characters written to the buffer, or zero if an error occurs. </returns>
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern uint FormatMessage(
        uint dwFlags,
        IntPtr lpSource,
        uint dwMessageId,
        uint dwLanguageId,
        ref IntPtr lpBuffer,
        uint nSize,
        IntPtr arguments
    );

    private Arguments _arguments;

    /// <summary>
    /// Sets the command line arguments for the logging process.
    /// </summary>
    /// <param name="arguments">The arguments to be used for logging operations.</param>
    public void SetArguments(Arguments arguments) => _arguments = arguments;

    private sealed class LogOptions
    {
        public string Message { get; init; } = string.Empty;
        public List<LogLevel> Level { get; init; } = [];
    }

    private void Log(LogOptions options)
    {
        try
        {
            bool debug = _arguments.debug;
            bool verbose = _arguments.verbose;

            bool shouldPrint = true;

            if (options.Level.Contains(LogLevel.Debug) && !debug)
            {
                shouldPrint = debug;
            }
            else if (options.Level.Contains(LogLevel.Verbose) && !verbose)
            {
                shouldPrint = verbose;
            }

            if (shouldPrint)
            {
                Console.WriteLine(options.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static string GetErrorMessage()
    {
        var errorCode = GetLastError();

        const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;

        var dwFlags =
            FORMAT_MESSAGE_ALLOCATE_BUFFER
            | FORMAT_MESSAGE_FROM_SYSTEM
            | FORMAT_MESSAGE_IGNORE_INSERTS;

        IntPtr messageBuffer = IntPtr.Zero;

        uint size = FormatMessage(
            dwFlags,
            IntPtr.Zero,
            errorCode,
            0,
            ref messageBuffer,
            0,
            IntPtr.Zero
        );

        var message = $"Unknown error (0x{errorCode:X})";

        if (size == 0)
            return message;

        message = Marshal.PtrToStringUni(messageBuffer) ?? message;

        Marshal.FreeHGlobal(messageBuffer);

        return message;
    }

    public void LogMulti(string input, params LogLevel[] levels) =>
        Log(new LogOptions { Message = input, Level = [.. levels] });

    public void LogDebug(string input) =>
        Log(new LogOptions { Message = input, Level = [LogLevel.Debug] });

    public void LogVerbose(string input) =>
        Log(new LogOptions { Message = input, Level = [LogLevel.Verbose] });

    public void LogInfo(string input) =>
        Log(new LogOptions { Message = input, Level = [LogLevel.Info] });

    public void LogError(Exception ex, string? message = null) =>
        Log(
            new LogOptions
            {
                Message = !string.IsNullOrEmpty(message) ? $"{message} : {ex}" : ex.ToString(),
                Level = [LogLevel.Error],
            }
        );

    public void LogError(string input) =>
        Log(new LogOptions { Message = input, Level = [LogLevel.Error] });

    public void LogError() =>
        Log(new LogOptions { Message = GetErrorMessage(), Level = [LogLevel.Error] });

    public void PrintApps()
    {
        var apps = ConfigurationsDefaults.Applications;
        foreach (var app in apps)
        {
            LogDebug("\n\n");
            LogDebug($"Name: {app.Name}");
            LogDebug($"Category: {app.Category}");
            LogDebug($"ExecutablePath: {app.ExecutablePath}");
            LogDebug($"WorkingDirectory: {app.WorkingDirectory}");
            LogDebug($"Arguments: {app.Arguments}");
            LogDebug($"ProcessName: {app.ProcessName}");
            LogDebug($"Order: {app.Order}");
            LogDebug($"IsActive: {app.IsActive}");
            LogDebug($"SkipRunningCheck: {app.SkipRunningCheck}");
            LogDebug($"MonitorIndex: {app.MonitorIndex}");
            LogDebug($"Position: {app.Position}");
            LogDebug($"Verb: {app.Verb}");
            LogDebug($"UseShellExecute: {app.UseShellExecute}");
            LogDebug($"CreateNoWindow: {app.CreateNoWindow}");
            LogDebug($"WindowStyle: {app.WindowStyle}");
            LogDebug($"WindowTitles: {string.Join(", ", app.WindowTitles)}");
            LogDebug($"SplashTitles: {string.Join(", ", app.SplashTitles)}");
        }
    }

    public void PrintDefaults()
    {
        LogDebug("\n\n");
        LogDebug($"WindowCaptureDelayMs: {ConfigurationsDefaults.WindowCaptureDelayMs}");
        LogDebug($"WindowCaptureRetries: {ConfigurationsDefaults.WindowCaptureRetries}");
        LogDebug($"DelayBeforeSnapMs: {ConfigurationsDefaults.DelayBeforeSnapMs}");
    }

    public void PrintMonitors(List<MonitorInfo> monitors)
    {
        LogDebug($"Found {monitors.Count} monitors.");
        var index = 0;
        foreach (var monitor in monitors)
        {
            LogInfo(
                $"\n\n Monitor {index++}: Resolution: {monitor.Width}x{monitor.Height}, IsPrimary: {monitor.IsPrimary}"
            );

            LogInfo($"\n    Bounds.Bottom: {monitor.Bounds.Bottom}");
            LogInfo($"    Bounds.Top: {monitor.Bounds.Top}");
            LogInfo($"    Bounds.Width: {monitor.Bounds.Width}");
            LogInfo($"    Bounds.Height: {monitor.Bounds.Height}");
            
            LogInfo($"\n    WorkArea.Bottom: {monitor.WorkArea.Bottom}");
            LogInfo($"    WorkArea.Top: {monitor.WorkArea.Top}");
            LogInfo($"    WorkArea.Width: {monitor.WorkArea.Width}");
            LogInfo($"    WorkArea.Height: {monitor.WorkArea.Height}");
        }
    }
}
