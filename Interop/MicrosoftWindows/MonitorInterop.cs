using System.Runtime.InteropServices;
using StartupScriptApp.Models;

namespace StartupScriptApp.Interop.MicrosoftWindows;

public static class MonitorInterop
{
    /// <summary>
    ///  Enumerates display monitors. An application can use the EnumDisplayMonitors function to enumerate the display monitors in a system. The function passes a handle to each display monitor and the monitor's rectangle to an application-defined callback function. If the application returns false from the callback function, the enumeration stops. If the application returns true, the enumeration continues until all display monitors have been enumerated or the callback function returns false.
    /// </summary>
    /// <param name="hdc"> A handle to a display device context that defines the visible region of interest. </param>
    /// <param name="lprcClip"> A pointer to a RECT structure that specifies a clipping rectangle. </param>
    /// <param name="lpfnEnum"> A pointer to an application-defined callback function. </param>
    /// <param name="dwData"> Application-defined data that is passed to the callback function. </param>
    /// <returns> True if successful; otherwise, false. </returns>
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumDisplayMonitors(
        IntPtr hdc,
        IntPtr lprcClip,
         EnumMonitorsDelegate lpfnEnum,
        IntPtr dwData
    );
    /// <summary>
    /// An application-defined callback function used with the EnumDisplayMonitors function. It receives a handle to a display monitor and the monitor's rectangle. The callback function should return true to continue enumeration or false to stop enumeration.
    /// </summary>
    /// <param name="hMonitor"> A handle to a display monitor. </param>
    /// <param name="hdcMonitor"> A handle to a device context. </param>
    /// <param name="lprcMonitor"> A pointer to a RECT structure. </param>
    /// <param name="dwData"> Application-defined data that is passed to the callback function. </param>
    /// <returns> True to continue enumeration; false to stop enumeration. </returns>
    public delegate bool EnumMonitorsDelegate(
        IntPtr hMonitor,
        IntPtr hdcMonitor,
        ref RECT lprcMonitor,
        IntPtr dwData
    );

    /// <summary>
    /// Retrieves information about a display monitor. The GetMonitorInfo function is typically called by an application that uses the EnumDisplayMonitors function to obtain a handle to a display monitor. The application can then call GetMonitorInfo to get information about the monitor, such as its size and position. The GetMonitorInfo function fills an OSMONITORINFO structure with information about the specified display monitor. The application must set the cbSize member of the OSMONITORINFO structure to sizeof(OSMONITORINFO) before calling GetMonitorInfo. This allows the function to determine the version of the structure being used and to provide compatibility with future versions of Windows.
    /// </summary>
    /// <param name="hMonitor"> A handle to the display monitor of interest </param>
    /// <param name="lpmi"> A pointer to an OSMONITORINFO structure that receives information about the specified display monitor. </param>
    /// <returns> True if successful; otherwise, false. </returns>
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetMonitorInfo(IntPtr hMonitor, ref OSMONITORINFO lpmi);
}

