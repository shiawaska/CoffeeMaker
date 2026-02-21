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

    public const int MONITORINFOF_PRIMARY = 0x00000001; // Is primary monitor
}
