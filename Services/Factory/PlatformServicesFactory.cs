using StartupScriptApp.Interfaces;
using StartupScriptApp.Services.Monitors.MicrosoftWindows;
using StartupScriptApp.Services.Windows.MicrosoftWindows;

namespace StartupScriptApp.Services.Factory;

public static class PlatformServicesFactory
{
    public static IMonitorManager CreateMonitorManager(Arguments args, ILogger logger)
    {
        if (OperatingSystem.IsWindows())
            return new WindowsMonitorManagement(args, logger);

        if (OperatingSystem.IsLinux())
            throw new NotImplementedException();
            // return new Services.Monitors.Linux.LinuxMonitorManagement(args, logger);

        throw new PlatformNotSupportedException("Monitor management is not supported on this OS.");
    }

    public static IWindowManager CreateWindowManager(ILogger logger, Arguments arguments)
    {
        if (OperatingSystem.IsWindows())
            return new WindowsWindowManagement(logger, arguments);

        if (OperatingSystem.IsLinux())
            throw new NotImplementedException();
            //return new Services.Linux.LinuxWindowManagement(logger);

        throw new PlatformNotSupportedException("Window management is not supported on this OS.");
    }
}