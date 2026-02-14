using System.Runtime.InteropServices;
using StartupScriptApp.models.Rect;

namespace StartupScriptApp.Models;

public class MonitorInfo
{
    public RECT Bounds { get; set; }
    public RECT WorkArea { get; set; }
    public int Width => Bounds.Right - Bounds.Left;
    public int Height => Bounds.Bottom - Bounds.Top;
}

[StructLayout(LayoutKind.Sequential)]
public struct OSMONITORINFO
{
    public uint cbSize;
    public RECT rcMonitor;
    public RECT rcWork;
    public uint dwFlags;
}