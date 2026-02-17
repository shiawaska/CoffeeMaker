using System.Runtime.InteropServices;
using StartupScriptApp.Enums;
using StartupScriptApp.Interfaces;
using StartupScriptApp.Models;
using StartupScriptApp.Models.ApplicationDefinition;

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

    private void Log(LogOptions options)
    {
        try
        {
            bool debug = _arguments.HasFlag("--debug");
            bool verbose = _arguments.HasFlag("--verbose") || _arguments.HasFlag("--v");

            if (options.VerboseGate is true && !verbose)
                return;

            if (options.VerboseGate is false && verbose)
                return;
            
            bool shouldPrint = true;
            
            if (_arguments.HasFlag("--debug"))
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

    public void LogDebug(string input, bool? verboseGate = null) =>
        Log(new LogOptions { Message = input, Level = [LogLevel.Debug] });

    public void LogDebug(string input, Area[] areas, bool? verboseGate = null) =>
        Log(
            new LogOptions
            {
                Message = input,
                Level = [LogLevel.Debug],
                Areas = [.. areas],
            }
        );

    public void LogInfo(string input, bool? verboseGate = null) =>
        Log(new LogOptions { Message = input, Level = [LogLevel.Info] });

    public void LogError() =>
        Log(new LogOptions { Message = GetErrorMessage(), Level = [LogLevel.Error] });

    public void LogError(Exception ex, string? message = null) =>
        Log(
            new LogOptions
            {
                Message = !string.IsNullOrEmpty(message) ? $"{message} : {ex}" : ex.ToString(),
                Level = [LogLevel.Error],
                VerboseGate = true
            }
        );

   public void LogError(string input, bool? verboseGate = null) =>
        Log(new LogOptions { Message = input + " Use --verbose to get more details", Level = [LogLevel.Error] });

    public void PrintApps(List<ApplicationDefinition> apps, string? headerMessage = null)
    {
        LogDebug("\n\n" + (headerMessage ?? "Applications"),[ Area.Application]);

        foreach (var app in apps)
        {
            LogDebug("\n", [ Area.Application]);
            LogDebug($"Name: {app.Name}", [ Area.Application]);
            LogDebug($"Category: {app.Category}", [ Area.Application]);
            LogDebug($"ExecutablePath: {app.ExecutablePath}", [ Area.Application]);
            LogDebug($"WorkingDirectory: {app.WorkingDirectory}", [ Area.Application]);
            LogDebug($"Arguments: {app.Arguments}", [ Area.Application]);
            LogDebug($"ProcessName: {app.ProcessName}", [ Area.Application]);
            LogDebug($"Order: {app.Order}", [Area.Application]);
            LogDebug($"IsActive: {app.IsActive}", [Area.Application]);
            LogDebug($"SkipRunningCheck: {app.SkipRunningCheck}", [Area.Application]);
            LogDebug($"MonitorIndex: {app.MonitorIndex}", [Area.Application]);
            LogDebug($"Position: {app.Position}", [Area.Application]);
            LogDebug($"Verb: {app.Verb}", [Area.Application]);
            LogDebug($"UseShellExecute: {app.UseShellExecute}", [Area.Application]);
            LogDebug($"CreateNoWindow: {app.CreateNoWindow}", [Area.Application]);
            LogDebug($"WindowStyle: {app.WindowStyle}", [Area.Application]);
            LogDebug($"WindowTitles: {string.Join(", ", app.WindowTitles)}", [Area.Application]);
            LogDebug($"SplashTitles: {string.Join(", ", app.SplashTitles)}", [Area.Application]);
        }
    }

    public void PrintDefaults()
    {
        LogDebug("\n\n");
        LogDebug($"WindowCaptureDelayMs: {ConfigurationsDefaults.WindowCaptureDelayMs}");
        LogDebug($"WindowCaptureRetries: {ConfigurationsDefaults.WindowCaptureRetries}");
        LogDebug($"DelayBeforeSnapMs: {ConfigurationsDefaults.DelayBeforeSnapMs}");
    }

    public void PrintMonitors(string headerMessage, List<MonitorInfo> monitors)
    {
        LogDebug($"\n\n{headerMessage}, Count: {monitors.Count}", [ Area.Monitor, Area.Config]);
        var index = 0;
        foreach (var monitor in monitors)
        {
            LogDebug(
                $"\n\n Monitor {index++}: Resolution: {monitor.Width}x{monitor.Height}, IsPrimary: {monitor.IsPrimary}", [ Area.Monitor, Area.Config]
            );

            LogDebug($"\n    Bounds.Bottom: {monitor.Bounds.Bottom}", [ Area.Monitor, Area.Config]);
            LogDebug($"    Bounds.Top: {monitor.Bounds.Top}", [ Area.Monitor, Area.Config]);
            LogDebug($"    Bounds.Width: {monitor.Bounds.Width}", [ Area.Monitor, Area.Config]);
            LogDebug($"    Bounds.Height: {monitor.Bounds.Height}", [ Area.Monitor, Area.Config]);

            LogDebug($"\n    WorkArea.Bottom: {monitor.WorkArea.Bottom}", [ Area.Monitor, Area.Config]);
            LogDebug($"    WorkArea.Top: {monitor.WorkArea.Top}", [ Area.Monitor, Area.Config]);
            LogDebug($"    WorkArea.Width: {monitor.WorkArea.Width}", [ Area.Monitor, Area.Config]);
            LogDebug($"    WorkArea.Height: {monitor.WorkArea.Height}", [ Area.Monitor, Area.Config]);
        }
    }
}

public class LogOptions
{
    public string Message { get; init; } = string.Empty;
    public List<LogLevel>? Level { get; init; } = [];
    public List<Area>? Areas { get; init; } = [];
    
    // null = any, true = only with --verbose, false = only without --verbose
    public bool? VerboseGate { get; init; } = null;
}
