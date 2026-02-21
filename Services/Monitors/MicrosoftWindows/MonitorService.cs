using System.Runtime.InteropServices;
using StartupScriptApp.Interfaces;
using StartupScriptApp.Interop.MicrosoftWindows;
using StartupScriptApp.Models;
using StartupScriptApp.Models.Constants;

namespace StartupScriptApp.Services.Monitors.MicrosoftWindows;

public class WindowsMonitorManagement(Arguments arguments, ILogger logger) : IMonitorManager
{
    private readonly bool _useDefaultMonitor = arguments.HasFlag("use-default-monitor");

    public List<MonitorInfo> GetAllMonitors()
    {
        return _useDefaultMonitor ? ConfigurationsDefaults.Monitors : GetOsMonitors();
    }

    private List<MonitorInfo> GetOsMonitors()
    {
        var list = new List<MonitorInfo>();
        var gcHandle = GCHandle.Alloc(list);

        try
        {
            MonitorInterop.EnumDisplayMonitors(
                IntPtr.Zero,
                IntPtr.Zero,
                MonitorInfoCallback,
                GCHandle.ToIntPtr(gcHandle)
            );
            list = OrderMonitorsLeftToRightTopToBottom(list);
            var index = 0;
            list.ForEach(monitor => monitor.Index = index++);
            logger.PrintMonitors("Monitors loaded from OS ", list);
            return list;
        }
        finally
        {
            gcHandle.Free();
        }
    }
    
    private static List<MonitorInfo> OrderMonitorsLeftToRightTopToBottom(List<MonitorInfo> monitors)
    {
        return monitors
            .OrderBy(m => m.Bounds.Top)
            .ThenBy(m => m.Bounds.Left)
            .ThenByDescending(m => m.IsPrimary)
            .ThenByDescending(m => (m.Bounds.Right - m.Bounds.Left) * (m.Bounds.Bottom - m.Bounds.Top))
            .ToList();
    }

    private bool MonitorInfoCallback(
        IntPtr hMonitor,
        IntPtr hdcMonitor,
        ref RECT lprcMonitor,
        IntPtr dwData
    )
    {
        var monitors = (List<MonitorInfo>)GCHandle.FromIntPtr(dwData).Target!;

        var mi = new OSMONITORINFO { cbSize = (uint)Marshal.SizeOf<OSMONITORINFO>() };

        if (MonitorInterop.GetMonitorInfo(hMonitor, ref mi))
            monitors.Add(
                new MonitorInfo
                {
                    Bounds = mi.rcMonitor,
                    WorkArea = mi.rcWork,
                    IsPrimary = mi.dwFlags == SnapConstants.MONITORINFOF_PRIMARY,
                }
            );
        else
        {
            logger.LogError();
        }
        return true;
    }
}
