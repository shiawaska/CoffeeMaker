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
        logger.LogDebug(
            $"\n\n Initiated search for window handles for application: {app.Name}",
            [Area.Window]
        );
        var processes = Process.GetProcessesByName(app.ProcessName).OrderBy(p => p.Id).ToArray();
        logger.LogDebug(
            $"Found {processes.Length} processes for application: {app.Name} with process name '{app.ProcessName}'",
            [Area.Window]
        );

        if (processes.Length == 0)
            return new List<IntPtr>();

        var retries = GetRetries(app);
        var handleValidationTasks = processes
            .Select(proc => ValidateWindowTitleAsync(proc, app.SplashTitles, retries))
            .ToList();

        var handles = await Task.WhenAll(handleValidationTasks);

        if (handles.All(h => h == IntPtr.Zero))
            logger.LogError($"No valid window handles found for application: {app.Name}");

        return handles.Where(h => h != IntPtr.Zero).Distinct().ToList();
    }

    public async Task<List<IntPtr>> FindWindowAsync(ApplicationDefinition app, Process proc)
    {
        await Task.Delay(1000);

        if (SafeCheckHasExited(proc))
            return await FindWindowAsync(app);

        var retries = GetRetries(app);

        var handles = await
            ValidateWindowTitleAsync(proc, app.SplashTitles, retries);
        if (handles == IntPtr.Zero)
        {
            logger.LogError(
                $"No valid window handles found for application: {app.Name}, Attempting Broader search"
            );
            return await FindWindowAsync(app);
        }

        return [handles];
    }

    public bool SetWindowPosition(IntPtr hWnd, SnapPosition snapPosition, MonitorInfo monitor)
    {
        var (bounds, state) = CalculateSnapSize(monitor, snapPosition);

        if (!IsWindowAccessible(hWnd))
        {
            logger.LogDebug($"\n Restoring window for handle: {hWnd}", [Area.Window], true);
            var restored = WindowInterop.ShowWindow(hWnd, SnapConstants.SW_RESTORE);
            if (!restored)
                logger.LogError();
            else
                logger.LogDebug($"Window position set on {hWnd}", [Area.Window], true);
        }

        logger.LogDebug($"\nCalling SetWindowPos for {hWnd}", [Area.Window], true);
        logger.LogDebug(
            $"With bounds: X: {bounds.X}, Y: {bounds.Y}, Width: {bounds.Width}, Height: {bounds.Height}",
            [Area.Window],
            true
        );

        var result = WindowInterop.SetWindowPos(
            hWnd,
            SnapConstants.HWND_TOP,
            bounds.X,
            bounds.Y,
            bounds.Width,
            bounds.Height,
            SnapConstants.SWP_SHOWWINDOW
        );

        if (!result)
            logger.LogError();
        else
            logger.LogDebug($"Window position set on {hWnd}", [Area.Window]);

        result = WindowInterop.ShowWindow(hWnd, state);
        if (!result)
            logger.LogError();
        else
            logger.LogDebug($"Window state set on {hWnd}", [Area.Window]);

        return result;
    }

    private (Rectangle, int) CalculateSnapSize(MonitorInfo monitor, SnapPosition position)
    {
        logger.LogDebug(
            $"\n\n Calculating snap size for position: {position.ToString()}",
            [Area.Window]
        );

        var bounds = position switch
        {
            SnapPosition.BottomLeft => new Rectangle(
                monitor.WorkArea.Left,
                monitor.WorkArea.Top + monitor.WorkArea.Height / 2,
                monitor.WorkArea.Width / 2,
                monitor.WorkArea.Height / 2
            ),
            SnapPosition.BottomRight => new Rectangle(
                monitor.WorkArea.Right - monitor.WorkArea.Width / 2,
                monitor.WorkArea.Top + monitor.WorkArea.Height / 2,
                monitor.WorkArea.Width / 2,
                monitor.WorkArea.Height / 2
            ),
            SnapPosition.TopLeft => new Rectangle(
                monitor.WorkArea.Left,
                monitor.WorkArea.Top,
                monitor.WorkArea.Width / 2,
                monitor.WorkArea.Height / 2
            ),
            SnapPosition.TopRight => new Rectangle(
                monitor.WorkArea.Right - monitor.WorkArea.Width / 2,
                monitor.WorkArea.Top,
                monitor.WorkArea.Width / 2,
                monitor.WorkArea.Height / 2
            ),
            SnapPosition.EntireLeft => new Rectangle(
                monitor.WorkArea.Left,
                monitor.WorkArea.Top,
                monitor.WorkArea.Width / 2,
                monitor.WorkArea.Height
            ),
            SnapPosition.EntireRight => new Rectangle(
                monitor.WorkArea.Right - monitor.WorkArea.Width / 2,
                monitor.WorkArea.Top,
                monitor.WorkArea.Width / 2,
                monitor.WorkArea.Height
            ),
            SnapPosition.EntireTop => new Rectangle(
                monitor.WorkArea.Left,
                monitor.WorkArea.Top,
                monitor.WorkArea.Width,
                monitor.WorkArea.Height / 2
            ),
            SnapPosition.EntireBottom => new Rectangle(
                monitor.WorkArea.Left,
                monitor.WorkArea.Bottom - monitor.WorkArea.Height / 2,
                monitor.WorkArea.Width,
                monitor.WorkArea.Height / 2
            ),
            SnapPosition.FullScreen => new Rectangle(
                monitor.WorkArea.Left,
                monitor.WorkArea.Top,
                monitor.WorkArea.Width,
                monitor.WorkArea.Height
            ),
            SnapPosition.Maximized => new Rectangle(
                monitor.WorkArea.Left,
                monitor.WorkArea.Top,
                monitor.WorkArea.Width,
                monitor.WorkArea.Height
            ),
            _ => new Rectangle(),
        };

        logger.LogDebug(
            $"Calculated snap size: X: {bounds.X}, Y: {bounds.Y}, Width: {bounds.Width}, Height: {bounds.Height}",
            [Area.Window],
            true
        );

        logger.LogDebug(
            $"Calculating window state for position: {position.ToString()}",
            [Area.Window]
        );

        int state = position switch
        {
            SnapPosition.Minimized => SnapConstants.SW_SHOWMINIMIZED,
            SnapPosition.Maximized => SnapConstants.SW_SHOWMAXIMIZED,
            _ => SnapConstants.SW_SHOWNORMAL,
        };
        logger.LogDebug($"Calculated window state: {nameof(state.ToString)}", [Area.Window], true);

        return (bounds, state);
    }

    private async Task<IntPtr> ValidateWindowTitleAsync(
        Process? proc,
        IReadOnlyList<string> splashTitles,
        int? retries = null,
        int? delayMs = null
    )
    {
        if (proc == null || SafeCheckHasExited(proc))
        {
            logger.LogDebug(
                $"Process: Id: {proc?.Id}, Name: {proc?.ProcessName}, Title: {proc?.MainWindowTitle} has exited during window validation.",
                [Area.Window]
            );
            return IntPtr.Zero;
        }

        retries ??= ConfigurationsDefaults.WindowCaptureRetries;
        delayMs ??= ConfigurationsDefaults.WindowCaptureDelayMs;

        if (retries <= 0)
        {
            logger.LogDebug(
                $"Process check retries exhausted for Id: {proc?.Id}, Name: {proc?.ProcessName}, Title: {proc?.MainWindowTitle}",
                [Area.Window]
            );
            return IntPtr.Zero;
        }
        await Task.Delay(delayMs.Value);
        if (retries != 3)
            proc?.Refresh();
        logger.LogDebug(
            $"Validating window title for process: {proc?.ProcessName}, Title: {proc?.MainWindowTitle}, Retries left: {retries}",
            [Area.Window]
        );

        if (
            !string.IsNullOrEmpty(proc.MainWindowTitle)
            && !splashTitles.Any(st =>
                proc.MainWindowTitle.Contains(st, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            logger.LogDebug(
                $"Found window with title: {proc?.MainWindowTitle} for process: {proc?.ProcessName}",
                [Area.Window]
            );
            return proc?.MainWindowHandle ?? IntPtr.Zero;
        }
        if (
            splashTitles.Any(st =>
                proc.MainWindowTitle.Contains(st, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            logger.LogDebug(
                $"Still on splash window for process: {proc?.ProcessName}, Title: {proc?.MainWindowTitle}, \n Retrying... ({retries} retries left)",
                [Area.Window]
            );

            return await ValidateWindowTitleAsync(proc, splashTitles, retries);
        }
        logger.LogDebug(
            $"Window title unavailable '{proc?.MainWindowTitle}' or is not a splash title for process: {proc?.ProcessName}, \n ",
            [Area.Window]
        );
        return await ValidateWindowTitleAsync(proc, splashTitles, retries - 1);
    }

    private int GetRetries(ApplicationDefinition app) =>
        app.ShouldRetry(arguments.ArgDict) ? ConfigurationsDefaults.WindowCaptureRetries : 0;

    private bool IsWindowAccessible(IntPtr hWnd)
    {
        if (WindowInterop.IsIconic(hWnd))
        {
            logger.LogDebug($"Window is minimized for handle: {hWnd}", true);
            return false;
        }

        if (WindowInterop.IsZoomed(hWnd))
        {
            logger.LogDebug($"Window is maximized for handle: {hWnd}", true);
            return false;
        }

        if (WindowInterop.IsWindowArranged(hWnd))
        {
            logger.LogDebug($"Window is in the background for handle: {hWnd}", true);
            return false;
        }

        logger.LogDebug($"Window is in the foreground for handle: {hWnd}", true);
        return true;
    }

    private bool SafeCheckHasExited(Process? proc)
    {
        var hasExited = true;
        if (proc == null)
            return hasExited;
        try
        {
            hasExited = proc.HasExited;
        }
        catch (Exception ex)
        {
            // this is usually due to a permissions issue.
            logger.LogError(
                $"Failed to check if process {proc.ProcessName} has exited. Is there a permission issue?",
                false
            );
            logger.LogError(ex, $"Failed to check if process {proc.ProcessName} has exited.");
        }
        return hasExited;
    }
}
