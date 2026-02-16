namespace StartupScriptApp.Models.Constants;

public static class SnapConstants
{
    public static int SWP_NOSIZE = 0x0001;
    public static int SWP_NOMOVE = 0x0002;
    public static int SWP_NOZORDER = 0x0004;
    public static int SWP_NOREDRAW = 0x0008;
    public static int SWP_NOACTIVATE = 0x0010;
    public static int SWP_FRAMECHANGED = 0x0020;
    public static int SWP_HIDEWINDOW = 0x0080;
    public static int SWP_NOCOPYBITS = 0x0100;
    public static int SWP_NOOWNERZORDER = 0x0200;
    public static int SWP_NOSENDCHANGING = 0x0400;
    public static int SWP_DRAWFRAME = SWP_FRAMECHANGED;
    public static int SWP_NOREPOSITION = SWP_NOOWNERZORDER;
    public static int SWP_DEFERERASE = 0x2000;
    public static int SWP_ASYNCWINDOWPOS = 0x4000;
    public static uint SWP_SHOWWINDOW = 0x0040;

    // Z-Order constants
    public static readonly IntPtr HWND_TOP = new IntPtr(0); // no change
    public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1); // Set top
    public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
    public static readonly IntPtr HWND_BOTTOM = new IntPtr(1); // Set bottom

    // System Metrics constants
    public const int SM_CXSCREEN = 0; // Primary monitor width
    public const int SM_CYSCREEN = 1; // Primary monitor height
    public const int SM_XVIRTUALSCREEN = 76; // Virtual screen left
    public const int SM_YVIRTUALSCREEN = 77; // Virtual screen top
    public const int SM_CXVIRTUALSCREEN = 78; // Virtual screen width
    public const int SM_CYVIRTUALSCREEN = 79; // Virtual screen height
    public const int SM_CMONITORS = 80; // Number of monitors
    
    //show window ncmd 
    public const int SW_HIDE = 0;
    public const int SW_SHOWNORMAL = 1;
    public const int SW_SHOWMINIMIZED = 2;
    public const int SW_SHOWMAXIMIZED = 3;
    public const int SW_SHOWNOACTIVATE = 4;
    public const int SW_SHOW = 5;
    public const int SW_MINIMIZE = 6;
    public const int SW_SHOWMINNOACTIVE = 7;
    public const int SW_SHOWNA = 8;
    public const int SW_RESTORE = 9;
    public const int SW_SHOWDEFAULT = 10;
    public const int SW_FORCEMINIMIZE = 11;

    public const int MONITORINFOF_PRIMARY = 0x00000001; // Is primary monitor
}
