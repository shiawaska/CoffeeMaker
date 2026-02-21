namespace StartupScriptApp.Models.Constants;

public class ZOrderConstants
{
    public static readonly IntPtr HWND_TOP = new IntPtr(0); // no change
    public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1); // Set top
    public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
    public static readonly IntPtr HWND_BOTTOM = new IntPtr(1); // Set bottom
}