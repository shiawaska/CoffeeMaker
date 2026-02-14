using System.Runtime.InteropServices;
using StartupScriptApp.Models;
using StartupScriptApp.models.Rect;

namespace StartupScriptApp.Interop;

public static class ExternalMethodDefinitions
{
     /// <summary>
    /// Retrieves the window handle used by the console associated with the calling process. If the calling process is not associated with a console, the return value is NULL. To get extended error information, call GetLastError.
    /// </summary>
    /// <returns> An <see cref="IntPtr"/> representing the window handle used by the console associated with the calling process. </returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetConsoleWindow();

    /// <summary>
    ///  Changes the size, position, and Z order of a child, pop-up, or top-level window. These windows are ordered according to their appearance on the screen. The topmost window receives the highest rank and is the first window in the Z order. The next highest window is the second in the Z order, and so on. Windows with the same Z order are sorted based on their creation time. The most recently created window is the topmost of those with the same Z order. If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
    /// </summary>
    /// <param name="hWnd"> A handle to a window. </param>
    /// <param name="hWndInsertAfter"> A handle to the window to precede the positioned window in the Z order. </param>
    /// <param name="X"> The new position of the left side of the window. </param>
    /// <param name="Y"> The new position of the top of the window. </param>
    /// <param name="cx"> The new width of the window. </param>
    /// <param name="cy"> The new height of the window. </param>
    /// <param name="uFlags"> The window sizing and positioning flags. </param>
    /// <returns> A <see cref="bool"/> indicating whether the operation was successful. </returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags
    );

    /// <summary>
    ///  Enumerates all top-level windows on the screen by passing the handle to each window, in turn, to an application-defined callback function. EnumWindows continues until the last top-level window is enumerated or the callback function returns false. If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call GetLastError.
    /// </summary>
    /// <param name="lpEnumFunc"> A pointer to an application-defined callback function. </param>
    /// <param name="lParam"> Application-defined data that is passed to the callback function. </param>
    /// <returns> A <see cref="bool"/> indicating whether the enumeration was successful. </returns>
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    /// <summary>
    ///     An application-defined callback function used with the EnumWindows function. It receives a handle to a top-level window and an application-defined value. The callback function should return true to continue enumeration or false to stop enumeration.
    /// </summary>
    /// <param name="hWnd"> A handle to a top-level window. </param>
    /// <param name="lParam"> Application-defined data that is passed to the callback function. </param>
    /// <returns> True to continue enumeration; false to stop enumeration. </returns>
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    /// <summary>
    /// Retrieves the identifier of the thread that created the specified window and, optionally, the identifier of the process that created the window. If the function succeeds, the return value is the identifier of the thread that created the window. If the function fails, the return value is zero. To get extended error information, call GetLastError.
    /// </summary>
    /// <param name="hWnd"> A handle to a window. </param>
    /// <param name="lpdwProcessId"> A pointer to a variable that receives the process identifier. </param>
    /// <returns> A <see cref="uint"/> representing the identifier of the thread that created the window. </returns>
    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    /// <summary>
    /// Determines the visibility state of the specified window. If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call GetLastError.
    /// </summary>
    /// <param name="hWnd"> A handle to a window. </param>
    /// <returns> True if the window is visible; otherwise, false. </returns>
    // UnmanagedType.Bool allows the return value to be marshaled as a boolean, where nonzero is true and zero is false. This is important for correctly interpreting the return value from the Windows API function, which typically returns a nonzero value for success and zero for failure.
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    /// <summary>
    ///  Retrieves a handle to a window that has the specified relationship (Z-Order or owner) to the specified window. The function searches only among top-level windows. To search among child windows, use the FindWindowEx function.
    /// </summary>
    /// <param name="hWnd"> A handle to a window. </param>
    /// <param name="uCmd"> The relationship between the specified window and the window whose handle is to be retrieved. </param>
    /// <returns> A handle to the window that has the specified relationship to the specified window. </returns>
    [DllImport("user32.dll")]
    public static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);

    /// <summary>
    ///  Enumerates display monitors. An application can use the EnumDisplayMonitors function to enumerate the display monitors in a system. The function passes a handle to each display monitor and the monitor's rectangle to an application-defined callback function. If the application returns false from the callback function, the enumeration stops. If the application returns true, the enumeration continues until all display monitors have been enumerated or the callback function returns false.
    /// </summary>
    /// <param name="hdc"> A handle to a display device context that defines the visible region of interest. </param>
    /// <param name="lprcClip"> A pointer to a RECT structure that specifies a clipping rectangle. </param>
    /// <param name="lpfnEnum"> A pointer to an application-defined callback function. </param>
    /// <param name="dwData"> Application-defined data that is passed to the callback function. </param>
    /// <returns> True if successful; otherwise, false. </returns>
    //UnmanagedType.Bool allows the return value to be marshaled as a boolean, where nonzero is true and zero is false. This is important for correctly interpreting the return value from the Windows API function, which typically returns a nonzero value for success and zero for failure.
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
    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getmonitorinfow
    // Charset.Auto allows the method to work correctly with both ANSI and Unicode versions of the Windows API
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetMonitorInfo(IntPtr hMonitor, ref OSMONITORINFO lpmi);
}