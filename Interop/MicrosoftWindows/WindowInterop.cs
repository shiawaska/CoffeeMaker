using System.Runtime.InteropServices;

namespace StartupScriptApp.Interop.Windows;

public class WindowInterop
{
    /// <summary>
    /// Retrieves the window handle used by the console associated with the calling process. If the calling process is not associated with a console, the return value is NULL. To get extended error information, call GetLastError.
    /// </summary>
    /// <returns> An <see cref="IntPtr"/> representing the window handle used by the console associated with the calling process. </returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetConsoleWindow();

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
    ///  An application-defined callback function used with the EnumWindows function. It receives a handle to a top-level window and an application-defined value. The callback function should return true to continue enumeration or false to stop enumeration.
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
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    /// <summary>
    ///  Determines whether the specified window is arranged (iconic).
    /// </summary>
    /// <param name="hWnd"> A handle to a window. </param>
    /// <returns> True if the window is arranged; otherwise, false. </returns>
    [DllImport("user32.dll")]
    public static extern bool IsWindowArranged(IntPtr hWnd);

    /// <summary>
    /// Determines whether the specified window is minimized.
    /// </summary>
    /// <param name="hWnd"> A handle to a window. </param>
    /// <returns> True if the window is minimized; otherwise, false. </returns>
    [DllImport("user32.dll")]
    public static extern bool IsIconic(IntPtr hWnd);

    /// <summary>
    /// Determines whether the specified window is maximized.
    /// </summary>
    /// <param name="hWnd"> A handle to a window. </param>
    /// <returns> True if the window is maximized; otherwise, false. </returns>
    [DllImport("user32.dll")]
    public static extern bool IsZoomed(IntPtr hWnd);
    
    /// <summary>
    ///  Activates the specified window and sets it as the foreground window.
    /// </summary>
    /// <param name="hWnd"> A handle to a window. </param>
    /// <param name="nCmdShow"> Specifies how the window is to be shown. This parameter is ignored the first time an application calls ShowWindow, if the program that launched the application provides a STARTUPINFO structure. Otherwise, the first time ShowWindow is called, the value should be the value obtained by the WinMain function in its nCmdShow parameter. In subsequent calls, this parameter can be one of the following values. </param>
    /// <returns> True if the window was successfully activated; otherwise, false. </returns>
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    /// <summary>
    ///  Retrieves a handle to a window that has the specified relationship (Z-Order or owner) to the specified window. The function searches only among top-level windows. To search among child windows, use the FindWindowEx function.
    /// </summary>
    /// <param name="hWnd"> A handle to a window. </param>
    /// <param name="uCmd"> The relationship between the specified window and the window whose handle is to be retrieved. </param>
    /// <returns> A handle to the window that has the specified relationship to the specified window. </returns>
    [DllImport("user32.dll")]
    public static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);

    /// <summary>
    ///  Moves the specified window to the specified screen coordinates. If the flag is set, the window is also raised to the top of the Z order.
    /// </summary>
    /// <param name="hWnd"> A handle to the window to be moved. </param>
    /// <param name="X"> The new position of the left side of the window. </param>
    /// <param name="Y"> The new position of the top of the window. </param>
    /// <param name="nWidth"> The new width of the window. </param>
    /// <param name="nHeight"> The new height of the window. </param>
    /// <param name="bRepaint"> Specifies whether the window is to be repainted. If this parameter is true, the window receives a message. If the parameter is false, no repainting of any kind occurs. This parameter does not affect child windows. </param>
    /// <returns> True if the window is moved successfully; otherwise, false. </returns>
    [DllImport("user32.dll")]
    public static extern bool MoveWindow(
        IntPtr hWnd,
        int X,
        int Y,
        int nWidth,
        int nHeight,
        bool bRepaint
    );

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
}
