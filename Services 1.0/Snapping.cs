using System.Diagnostics;
using System.Runtime.InteropServices;
using StartupScriptApp.Enums.SnapPosition;
using StartupScriptApp.Interfaces;
using StartupScriptApp.Interop;
using StartupScriptApp.Models;
using StartupScriptApp.models.ApplicationDefinition;
using StartupScriptApp.Models.Configurations;
using StartupScriptApp.Models.Constants;
using StartupScriptApp.models.Rect;

namespace StartupScriptApp.Services;

public class Snapping(ILogger logger, Arguments arguments)
{
    /// <summary>
    /// Gets the list of monitors either from the OS or from predefined configurations based on the useOsMonitors flag.
    /// </summary>
    /// <param name="useDefinedMonitors"> If true, retrieves monitor information from predefined configurations; otherwise, uses the operating system's monitor information. </param>
    /// <returns>A list of MonitorInfo objects representing the monitors.</returns>
    public List<MonitorInfo> GetMonitors(bool useDefinedMonitors)
    {
        if (useDefinedMonitors)
            return ToMonitorInfoList();
        return GetMonitorsFromOs();
    }

    /// <summary>
    /// Retrieves monitor information directly from the operating system using EnumDisplayMonitors and GetMonitorInfo.
    /// </summary>
    /// <returns>A list of MonitorInfo objects representing the monitors detected by the OS.</returns>
    private List<MonitorInfo> GetMonitorsFromOs()
    {
        var list = new List<MonitorInfo>();
        
        
        
        
        ExternalMethodDefinitions.EnumDisplayMonitors(
            IntPtr.Zero,
            IntPtr.Zero,
            (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
            {
                var mi = new OSMONITORINFO { cbSize = (uint)Marshal.SizeOf<OSMONITORINFO>() };
                if (ExternalMethodDefinitions.GetMonitorInfo(hMonitor, ref mi))
                    list.Add(new MonitorInfo { Bounds = mi.rcMonitor, WorkArea = mi.rcWork });
                return true;
            },
            IntPtr.Zero
        );
        list.Sort((a, b) => a.Bounds.Left.CompareTo(b.Bounds.Left));
        return list;
    }

    private MonitorInfo ToMonitorInfo(MonitorConfig config)
    {
        return new MonitorInfo
        {
            Bounds = new RECT
            {
                Left = config.X,
                Top = config.Y,
                Right = config.X + config.Width,
                Bottom = config.Y + config.Height,
            },
            WorkArea = new RECT
            {
                Left = config.X,
                Top = config.Y,
                Right = config.X + config.Width,
                Bottom = config.Y + config.Height,
            },
        };
    }

    private List<MonitorInfo> ToMonitorInfoList()
    {
        var list = new List<MonitorInfo>();
        foreach (var config in ConfigurationsDefaults.Monitors)
        {
            list.Add(ToMonitorInfo(config));
        }
        return list;
    }
    public List<MonitorConfig> GetMonitorConfigs(bool useOsMonitors)
    {
        if (useOsMonitors)
        {
            return GetOsMonitors();
        }
        return ConfigurationsDefaults.Monitors;
    }

    private List<MonitorConfig> GetOsMonitors()
    {
        var osMonitors = new List<MonitorInfo>();
        // osMonitors = MonitorLayout.GetMonitorsFromOs();
        var configs = new List<MonitorConfig>();

        for (int i = 0; i < osMonitors.Count; i++)
        {
            var monitor = osMonitors[i];
            configs.Add(
                new MonitorConfig
                {
                    Id = $"os-monitor-{i + 1}",
                    Name = $"Display {i + 1}",
                    Width = monitor.Width,
                    Height = monitor.Height,
                    X = monitor.Bounds.Left,
                    Y = monitor.Bounds.Top,
                    IsPrimary = i == 0, // First monitor is typically primary
                }
            );
        }

        return configs;
    }
    
    /// <summary>
    ///  Finds a top-level window for the given app by checking process names and window titles, with retries to handle splash screens. Returns IntPtr.Zero if not found after retries.
    /// </summary>
    /// <param name="app"> The ApplicationDefinition instance containing process and window title information. </param>
    /// <returns> The handle to the found window, or IntPtr.Zero if not found. </returns>
    public async Task<List<IntPtr>> FindWindowsAsync(ApplicationDefinition app)
    {
        foreach (var proc in Process.GetProcessesByName(app.ProcessName))
        {
            logger.DebugLog($"Checking process: {proc.ProcessName} for app: {app.Name}");

            List<Task<IntPtr>> ProcessChecks = new List<Task<IntPtr>>();
            foreach (var windowTitle in app.WindowTitles)
                ProcessChecks.Add(ProcessCheck(proc, windowTitle, app.SplashTitles));
            var handles = await Task.WhenAll(ProcessChecks);

            var validHandles = handles.Where(h => h != IntPtr.Zero).Distinct().ToList();
            if (validHandles.Count > 0)
            {
                logger.DebugLog(
                    $"debug @@ Found matching window(s) for process: {proc.ProcessName}, Title: {proc.MainWindowTitle}, Handles: {string.Join(", ", validHandles)}"
                );
                return validHandles; // Return the list of matching window handles
            }
            else
            {
                logger.DebugLog(
                    $"No matching window found for process: {proc.ProcessName}, Title: {proc.MainWindowTitle}"
                );
            }
        }
        return new List<IntPtr> { IntPtr.Zero };
    }

    private async Task<IntPtr> ProcessCheck(
        Process proc,
        string windowTitle,
        IReadOnlyList<string> splashTitles,
        int? retries = null,
        int? delayMs = null
    )
    {
        retries ??= ConfigurationsDefaults.ProcessCheckRetries;
        delayMs ??= ConfigurationsDefaults.DelayBeforeSnapMs;
        
        if (retries <= 0)
        {
            logger.DebugLog(
                $"Process check retries exhausted for process: {proc.ProcessName}, Title: {proc.MainWindowTitle}"
            );
            return IntPtr.Zero;
        }
        await Task.Delay(delayMs.Value);
        if (retries != 3)
            proc.Refresh();
        logger.DebugLog(
            $"Checking window title for process: {proc.ProcessName}, Title: {proc.MainWindowTitle}, Retries left: {retries}"
        );
        if (
            !string.IsNullOrEmpty(proc.MainWindowTitle)
            && proc.MainWindowTitle.Contains(windowTitle, StringComparison.OrdinalIgnoreCase)
            && !splashTitles.Any(st =>
                proc.MainWindowTitle.Contains(st, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            logger.DebugLog(
                $"Found window with title: {proc.MainWindowTitle} for process: {proc.ProcessName}"
            );
            return proc.MainWindowHandle;
        }
        else if (
            splashTitles.Any(st =>
                proc.MainWindowTitle.Contains(st, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            logger.DebugLog(
                $"Still on splash window for process: {proc.ProcessName}, Title: {proc.MainWindowTitle}, \n Retrying... ({retries - 1} retries left)"
            );
            logger.DebugLog(
                $"Refreshing process info for: {proc.ProcessName} before retrying window check."
            );
            return await ProcessCheck(proc, windowTitle, splashTitles, retries - 1);
        }
        logger.DebugLog(
            $"Window title '{proc.MainWindowTitle}' does not match expected title '{windowTitle}' and is not a splash title for process: {proc.ProcessName}, \n Retrying... ({retries - 1} retries left)"
        );
        return await ProcessCheck(proc, windowTitle, splashTitles, retries - 1);
    }

    /// <summary>
    /// Calculates the RECT for snapping a window to a specific position on a given monitor.
    /// </summary>
    /// <param name="monitor"> The monitor information including work area. </param>
    /// <param name="position"> The desired snap position on the monitor. </param>
    /// <returns> The RECT representing the area for the snap position on the monitor. </returns>
    public RECT GetRectForSnap(MonitorInfo monitor, SnapPosition position)
    {
        var work = monitor.WorkArea;
        int w = work.Right - work.Left,
            h = work.Bottom - work.Top;
        int halfW = w / 2,
            halfH = h / 2;
        return position switch
        {
            SnapPosition.TopLeft => new RECT
            {
                Left = work.Left,
                Top = work.Top,
                Right = work.Left + halfW,
                Bottom = work.Top + halfH,
            },
            SnapPosition.TopRight => new RECT
            {
                Left = work.Left + halfW,
                Top = work.Top,
                Right = work.Right,
                Bottom = work.Top + halfH,
            },
            SnapPosition.BottomLeft => new RECT
            {
                Left = work.Left,
                Top = work.Top + halfH,
                Right = work.Left + halfW,
                Bottom = work.Bottom,
            },
            SnapPosition.BottomRight => new RECT
            {
                Left = work.Left + halfW,
                Top = work.Top + halfH,
                Right = work.Right,
                Bottom = work.Bottom,
            },
            SnapPosition.EntireLeft => new RECT
            {
                Left = work.Left,
                Top = work.Top,
                Right = work.Left + halfW,
                Bottom = work.Bottom,
            },
            SnapPosition.EntireRight => new RECT
            {
                Left = work.Left + halfW,
                Top = work.Top,
                Right = work.Right,
                Bottom = work.Bottom,
            },
            SnapPosition.EntireTop => new RECT
            {
                Left = work.Left,
                Top = work.Top,
                Right = work.Right,
                Bottom = work.Top + halfH,
            },
            SnapPosition.EntireBottom => new RECT
            {
                Left = work.Left,
                Top = work.Top + halfH,
                Right = work.Right,
                Bottom = work.Bottom,
            },
            SnapPosition.FullScreen => work,
            _ => work,
        };
    }

    /// <summary>
    /// Snaps the specified window to a given monitor and position.
    /// </summary>
    /// <param name="hWnd"> The handle to the window to snap. </param>
    /// <param name="monitorIndex1Based"> The 1-based index of the target monitor in the monitors list. </param>
    /// <param name="position"> The desired snap position on the monitor. </param>
    /// <param name="monitors"> The list of available monitors. </param>
    /// <returns> True if the window was successfully snapped; otherwise, false. </returns>
    public bool SnapWindow(
        IntPtr hWnd,
        int monitorIndex1Based,
        SnapPosition position,
        List<MonitorInfo> monitors,
        ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal
    )
    {
        if (monitorIndex1Based < 1 || monitorIndex1Based > monitors.Count)
            return false;

        var monitor = monitors[monitorIndex1Based - 1];

        var visibilityFlag =
            windowStyle == ProcessWindowStyle.Minimized
                ? SnapConstants.SWPFlags.SWP_HIDEWINDOW
                : SnapConstants.SWPFlags.SWP_SHOWWINDOW;

        var swpFlags = (uint)SnapConstants.SWPFlags.SWP_NOZORDER | (uint)visibilityFlag;

        var r = GetRectForSnap(monitor, position);
        try
        {
            var result = ExternalMethodDefinitions.SetWindowPos(
                hWnd,
                SnapConstants.HWND_NOTOPMOST,
                r.Left,
                r.Top,
                r.Width,
                r.Height,
                swpFlags
            );
            if (!result)
            {
                int error = Marshal.GetLastWin32Error();
                logger.InfoLog($"SetWindowPos failed with error code: {error}");
            }
            return result;
        }
        catch (Exception ex)
        {
            logger.DebugLog($"Error in SetWindowPos: {ex.Message}");
            return false;
        }
    }
    
    public async Task ExecuteWindowSnap(ApplicationDefinition app)
    {
        var debug = arguments.debug;
        try
        {
            var useDefinedMonitors = arguments.HasFlag("use-defined-monitors");
            var monitors = GetMonitors(useDefinedMonitors);

            var windows = await FindWindowsAsync(app);
            foreach (var window in windows)
                logger.DebugLog($"{app.Name}: Window handle: 0x{window.ToInt64():X}");

            if (
                (windows.Count == 0 || windows.All(h => h == IntPtr.Zero))
                && arguments.argDict.ContainsKey("seq")
            )
            {
                logger.DebugLog($"Trying console window for {app.Name}...");
                windows = [ExternalMethodDefinitions.GetConsoleWindow()];
                foreach (var window in windows)
                    logger.DebugLog(
                        $"{app.Name}: Using console window handle: 0x{window.ToInt64():X}"
                    );
            }

            if (windows.Count == 0 || windows.All(h => h == IntPtr.Zero))
            {
                logger.InfoLog($"\nCould not find window for {app.Name} to snap.");
                return;
            }
            foreach (var window in windows)
            {
                logger.DebugLog(
                    $"{app.Name}: Attempting to snap window handle: 0x{window.ToInt64():X}" 
                );
                bool result = SnapWindow(
                    window,
                    app.MonitorIndex,
                    app.Position,
                    monitors,
                    app.WindowStyle
                );
                if (result)
                    logger.InfoLog(
                        $"\nSnapped {app.Name} to monitor {app.MonitorIndex}, {app.Position}."
                    );
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    logger.InfoLog($"\nFailed to snap {app.Name} window. Error code: {error}");
                }
            }
        }
        catch (Exception ex)
        {
            logger.ErrorLog($"\nError snapping {app.Name}: {ex.Message}");
        }
    }

}