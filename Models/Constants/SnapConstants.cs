namespace StartupScriptApp.Models.Constants;

public class SnapConstants
{
    [Flags]
    public enum SWPFlags : uint
    {
        SWP_NOSIZE = 0x0001,
        SWP_NOMOVE = 0x0002,
        SWP_NOZORDER = 0x0004,
        SWP_NOREDRAW = 0x0008,
        SWP_NOACTIVATE = 0x0010,
        SWP_FRAMECHANGED = 0x0020,
        SWP_SHOWWINDOW = 0x0040,
        SWP_HIDEWINDOW = 0x0080,
        SWP_NOCOPYBITS = 0x0100,
        SWP_NOOWNERZORDER = 0x0200,
        SWP_NOSENDCHANGING = 0x0400,
        SWP_DRAWFRAME = SWP_FRAMECHANGED,
        SWP_NOREPOSITION = SWP_NOOWNERZORDER,
        SWP_DEFERERASE = 0x2000,
        SWP_ASYNCWINDOWPOS = 0x4000,
    }

    // Z-Order constants
    public static readonly IntPtr HWND_TOP = new IntPtr(0);
    public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
    public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

    // System Metrics constants
    public const int SM_CXSCREEN = 0; // Primary monitor width
    public const int SM_CYSCREEN = 1; // Primary monitor height
    public const int SM_XVIRTUALSCREEN = 76; // Virtual screen left
    public const int SM_YVIRTUALSCREEN = 77; // Virtual screen top
    public const int SM_CXVIRTUALSCREEN = 78; // Virtual screen width
    public const int SM_CYVIRTUALSCREEN = 79; // Virtual screen height
    public const int SM_CMONITORS = 80; // Number of monitors


}