using StartupScriptApp.Interfaces;
using StartupScriptApp.Interop.Interfaces;
using StartupScriptApp.Models;
using StartupScriptApp.Models.ApplicationDefinition;
using StartupScriptApp.Services;
using StartupScriptApp.Services.Factory;

namespace StartupScriptApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        var (logger, arguments, monitorManager, processManager, windowManager, config) =
            GetServices(args);

        await DelayCheck(arguments, logger);

        var path = BuildConfigFilePath(arguments, logger);

        config.InitializeFromJson(path);

        var monitors = monitorManager.GetAllMonitors();

        var apps = ConfigurationsDefaults.Applications;

        await StartApps(apps, monitors, windowManager, processManager, arguments);
    }

    private static string BuildConfigFilePath(Arguments arguments, ILogger logger)
    {
        var path = ConfigurationsDefaults.DefaultConfigFilePath;

        if (arguments.HasFlag("path"))
        {
            arguments.ArgDict.TryGetValue("path", out var customPath);
            if (!string.IsNullOrEmpty(customPath?[0]))
                path = customPath[0];
        }
        logger.LogDebug($"{path}");

        return path;
    }

    private static async Task StartApp(
        ApplicationDefinition app,
        List<MonitorInfo> monitors,
        IWindowManager windowManager,
        ProcessManagement processManager,
        Arguments arguments
    )
    {
        var proceed = processManager.ProcessStartupChecklistAsync(app, arguments.ArgDict);
        if (!proceed)
            return;
        var process = processManager.StartProcess(app.ToProcessStartInfo());
        await Task.Delay(ConfigurationsDefaults.DelayBeforeSnapMs);

        List<IntPtr> windowHandle = new List<IntPtr>();

        if (process == null && !String.IsNullOrEmpty(app.ProcessName))
            windowHandle = await windowManager.FindWindowAsync(app);
        else if (process != null)
            windowHandle = await windowManager.FindWindowAsync(app, process);

        if (windowHandle.All(p => p == IntPtr.Zero))
            return;
        windowHandle.ForEach(wh =>
            windowManager.SetWindowPosition(wh, app.Position, monitors[app.MonitorIndex - 1])
        );
    }

    private static async Task StartAppsInParallel(
        List<ApplicationDefinition> apps,
        List<MonitorInfo> monitors,
        IWindowManager windowManager,
        ProcessManagement processManager,
        Arguments arguments
    )
    {
        var appTasks = apps.Select(app =>
            Task.Run(async () =>
            {
                await StartApp(app, monitors, windowManager, processManager, arguments);
            })
        );
        await Task.WhenAll(appTasks);
    }

    private static async Task StartAppInSequence(
        List<ApplicationDefinition> app,
        List<MonitorInfo> monitors,
        IWindowManager windowManager,
        ProcessManagement processManager,
        Arguments arguments
    )
    {
        foreach (var a in app)
        {
            await StartApp(a, monitors, windowManager, processManager, arguments);
        }
    }

    private static async Task StartApps(
        List<ApplicationDefinition> apps,
        List<MonitorInfo> monitors,
        IWindowManager windowManager,
        ProcessManagement processManager,
        Arguments arguments
    )
    {
        var sequential = arguments.HasFlag("--seq");
        if (sequential)
            await StartAppInSequence(apps, monitors, windowManager, processManager, arguments);
        else
            await StartAppsInParallel(apps, monitors, windowManager, processManager, arguments);
    }

    private static async Task DelayCheck(Arguments arguments, ILogger logger)
    {
        if (arguments.HasFlag("--delay"))
        {
            var delay = arguments.ArgDict["delay"][0];

            var success = int.TryParse(delay, out var delayInt);
            if (!success)
            {
                logger.LogError("Invalid delay value provided");
                return;
            }

            await Task.Delay(delayInt);
        }
    }

    private static (
        ILogger,
        Arguments,
        IMonitorManager,
        ProcessManagement,
        IWindowManager,
        Configuration
    ) GetServices(string[] args)
    {
        var logger = new Logging();
        var arguments = new Arguments(logger);
        logger.SetArguments(arguments);
        arguments.SetArgs(args);
        IMonitorManager monitorManager = PlatformServicesFactory.CreateMonitorManager(
            arguments,
            logger
        );
        var processManager = new ProcessManagement(logger);
        IWindowManager windowManager = PlatformServicesFactory.CreateWindowManager(
            logger,
            arguments
        );
        var config = new Configuration(logger);
        return (logger, arguments, monitorManager, processManager, windowManager, config);
    }
}
