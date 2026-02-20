using System.Runtime.InteropServices;
using StartupScriptApp.Interfaces;
using StartupScriptApp.Interop.Interfaces;
using StartupScriptApp.Interop.Windows;
using StartupScriptApp.Models;
using StartupScriptApp.Models.Constants;

namespace StartupScriptApp.Services.Monitors.MicrosoftWindows;

public class WindowsMonitorManagement(Arguments arguments, ILogger logger) : IMonitorManager
{
    // Top left is 0,0
    // Bottom right is Width, Height

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

    /// <summary>
    /// Depricated, Needs more testing but trust the order returned as the left to right, top to buttom list.
    /// </summary>
    /// <param name="monitors"></param>
    /// <returns></returns>
    private List<MonitorInfo> OrganizeMonitors(List<MonitorInfo> monitors)
    {
        var list = monitors;
        
        list.Sort(
            (a, b) =>
            {
                var topCompare = a.Bounds.Top.CompareTo(b.Bounds.Top);
                return topCompare != 0 ? topCompare : a.Bounds.Left.CompareTo(b.Bounds.Left);
            }
        );
        
        return list;
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
