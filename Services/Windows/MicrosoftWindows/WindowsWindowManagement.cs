using System.Diagnostics;
using System.Drawing;
using StartupScriptApp.Enums;
using StartupScriptApp.Extensions;
using StartupScriptApp.Interfaces;
using StartupScriptApp.Interop.Windows;
using StartupScriptApp.Models;
using StartupScriptApp.Models.ApplicationDefinition;
using StartupScriptApp.Models.Constants;

namespace StartupScriptApp.Services.Windows.MicrosoftWindows;

public class WindowsWindowManagement(ILogger logger, Arguments arguments) : IWindowManager
{
    public async Task<List<IntPtr>> FindWindowAsync(ApplicationDefinition app)
    {
        logger.LogDebug($"\n\n Initiated search for window handles for application: {app.Name}");
        var processes = Process.GetProcessesByName(app.ProcessName).OrderBy(p => p.Id).ToArray();
        logger.LogDebug($"Found {processes.Length} processes for application: {app.Name}");
        foreach (var proc in processes)
        {
            try
            {
                if (proc.HasExited)
                    continue;
            }
            catch (Exception ex)
            {
                // this is usually due to a permissions issue.
                logger.LogMulti($"Failed to get process info for process: {proc.Id} : {ex}", LogLevel.Verbose);
                continue;
            }

            if (String.IsNullOrEmpty(proc.MainWindowTitle))
                continue;
            logger.LogDebug($"Process: {proc.Id}, MainWindowTitle: '{proc.MainWindowTitle}'");
        }
        if (processes.Length == 0)
            return new List<IntPtr>();


        var retries = GetRetries(app);
        var handleValidationTasks = processes
            .SelectMany(
                proc => app.WindowTitles,
                (proc, windowTitle) => ValidateWindowTitleAsync(proc, windowTitle, app.SplashTitles, retries)
            )
            .ToList();

        var handles = await Task.WhenAll(handleValidationTasks);
        if (handles.All(h => h == IntPtr.Zero))
            logger.LogError($"No valid window handles found for application: {app.Name}");

        return handles.Where(h => h != IntPtr.Zero).Distinct().ToList();
    }

    public async Task<List<IntPtr>> FindWindowAsync(ApplicationDefinition app, Process proc)
    {
        if (proc.HasExited)
            return await FindWindowAsync(app);

        var retries = GetRetries(app);
        var handles = await Task.WhenAll(
            app.WindowTitles.Select(wt => ValidateWindowTitleAsync(proc, wt, app.SplashTitles, retries))
                .ToList()
        );

        if (handles.All(h => h == IntPtr.Zero))
        {
            logger.LogError($"No valid window handles found for application: {app.Name}");
            if (Arguments.HasFlag("ext-search", new Arguments(logger)))
            {
                return await FindWindowAsync(app);
            }
        }

        return handles.Where(h => h != IntPtr.Zero).Distinct().ToList();
    }

    public bool SetWindowPosition(
        IntPtr hWnd,
        SnapPosition snapPosition,
        MonitorInfo monitor,
        int? zOrder
    )
    {
        var bounds = CalculateSnapSize(monitor, snapPosition);

        logger.LogDebug($"\n Calling SetWindowPos for {hWnd}");
        var uflag = GetWindowState(hWnd);

        if (uflag != SnapConstants.SW_SHOWNORMAL)
        {
            WindowInterop.ShowWindow(hWnd, SnapConstants.SW_RESTORE);
        }


        var result = WindowInterop.SetWindowPos(
            hWnd,
            zOrder.HasValue ? new IntPtr(zOrder.Value) : SnapConstants.HWND_TOP,
            bounds.X,
            bounds.Y,
            bounds.Width,
            bounds.Height,
            uflag
        );


        if (!result)
            logger.LogError();

        logger.LogDebug($"SetWindowPos actioned on {hWnd}");
        return result;
    }

    private Rectangle CalculateSnapSize(MonitorInfo monitor, SnapPosition position)
    {
        logger.LogDebug($"\n\n Calculating snap size for position: {position.ToString()}");

        var bounds = position switch
        {
            SnapPosition.BottomLeft => new Rectangle(
                monitor.Bounds.Left,
                monitor.Bounds.Top + monitor.Bounds.Height / 2,
                monitor.Bounds.Width / 2,
                monitor.Bounds.Height / 2
            ),
            SnapPosition.BottomRight => new Rectangle(
                monitor.Bounds.Right - monitor.Bounds.Width / 2,
                monitor.Bounds.Top + monitor.Bounds.Height / 2,
                monitor.Bounds.Width / 2,
                monitor.Bounds.Height / 2
            ),
            SnapPosition.TopLeft => new Rectangle(
                monitor.Bounds.Left,
                monitor.Bounds.Top,
                monitor.Bounds.Width / 2,
                monitor.Bounds.Height / 2
            ),
            SnapPosition.TopRight => new Rectangle(
                monitor.Bounds.Right - monitor.Bounds.Width / 2,
                monitor.Bounds.Top,
                monitor.Bounds.Width / 2,
                monitor.Bounds.Height / 2
            ),
            SnapPosition.EntireLeft => new Rectangle(
                monitor.Bounds.Left,
                monitor.Bounds.Top,
                monitor.Bounds.Width / 2,
                monitor.Bounds.Height
            ),
            SnapPosition.EntireRight => new Rectangle(
                monitor.Bounds.Right - monitor.Bounds.Width / 2,
                monitor.Bounds.Top,
                monitor.Bounds.Width / 2,
                monitor.Bounds.Height
            ),
            SnapPosition.EntireTop => new Rectangle(
                monitor.Bounds.Left,
                monitor.Bounds.Top,
                monitor.Bounds.Width,
                monitor.Bounds.Height / 2
            ),
            SnapPosition.EntireBottom => new Rectangle(
                monitor.Bounds.Left,
                monitor.Bounds.Bottom - monitor.Bounds.Height / 2,
                monitor.Bounds.Width,
                monitor.Bounds.Height / 2
            ),
            SnapPosition.FullScreen => new Rectangle(
                monitor.Bounds.Left,
                monitor.Bounds.Top,
                monitor.Bounds.Width,
                monitor.Bounds.Height
            ),
            _ => new Rectangle(),
        };

        logger.LogDebug(
            $"Calculated snap size: X: {bounds.X}, Y: {bounds.Y}, Width: {bounds.Width}, Height: {bounds.Height}"
        );
        return bounds;
    }

    private async Task<IntPtr> ValidateWindowTitleAsync(
        Process? proc,
        string? windowTitle,
        IReadOnlyList<string> splashTitles,
        int? retries = null,
        int? delayMs = null
    )
    {
        logger.LogDebug(
            $"Validating window title for process: {proc?.ProcessName}, Title: {proc?.MainWindowTitle}"
        );
        if (proc == null)
            return IntPtr.Zero;
        retries ??= ConfigurationsDefaults.WindowCaptureRetries;
        delayMs ??= ConfigurationsDefaults.WindowCaptureDelayMs;

        if (retries <= 0)
        {
            logger.LogDebug(
                $"Process check retries exhausted for process: {proc.ProcessName}, Title: {proc.MainWindowTitle}"
            );
            return IntPtr.Zero;
        }
        await Task.Delay(delayMs.Value);
        if (retries != 3)
            proc.Refresh();
        logger.LogDebug(
            $"Validating window title for process: {proc.ProcessName}, Title: {proc.MainWindowTitle}, Retries left: {retries}"
        );

        if (
            !string.IsNullOrEmpty(proc.MainWindowTitle)
            &&
            // Removed this due to window titles being unreliable. Only process with a MainWindowTitle will have a window associated with it. So this will match all windows associated with the process

            /* (
                string.IsNullOrEmpty(windowTitle)
             || proc.MainWindowTitle.Contains(windowTitle, StringComparison.OrdinalIgnoreCase)
            )
            &&*/ !splashTitles.Any(st =>
                proc.MainWindowTitle.Contains(st, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            logger.LogDebug(
                $"Found window with title: {proc.MainWindowTitle} for process: {proc.ProcessName}"
            );
            return proc.MainWindowHandle;
        }
        if (
            splashTitles.Any(st =>
                proc.MainWindowTitle.Contains(st, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            logger.LogDebug(
                $"Still on splash window for process: {proc.ProcessName}, Title: {proc.MainWindowTitle}, \n Retrying... ({retries - 1} retries left)"
            );
            logger.LogDebug(
                $"Refreshing process info for: {proc.ProcessName} before retrying window check."
            );
            return await ValidateWindowTitleAsync(proc, windowTitle, splashTitles, retries - 1);
        }
        logger.LogDebug(
            $"Window title '{proc.MainWindowTitle}' does not match expected title '{windowTitle}' and is not a splash title for process: {proc.ProcessName}, \n Retrying... ({retries - 1} retries left)"
        );
        return await ValidateWindowTitleAsync(proc, windowTitle, splashTitles, retries - 1);
    }

    private int GetRetries(ApplicationDefinition app) => app.ShouldRetry(arguments.argDict) ? ConfigurationsDefaults.WindowCaptureRetries : 0;

    private uint GetWindowState(IntPtr hWnd)
    {
        if (WindowInterop.IsIconic(hWnd))
            return SnapConstants.SW_SHOWMINIMIZED;
        if (WindowInterop.IsZoomed(hWnd))
            return SnapConstants.SW_SHOWMAXIMIZED;
        if (WindowInterop.IsWindowArranged(hWnd))
            return SnapConstants.SW_SHOWMINNOACTIVE;
        return SnapConstants.SW_SHOWNORMAL;
    }
}
