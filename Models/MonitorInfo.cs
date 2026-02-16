using System.Runtime.InteropServices;

namespace StartupScriptApp.Models;

/// <summary>
/// Represents information about a monitor.
/// </summary>
public class MonitorInfo
{
    public RECT Bounds { get; set; }
    public RECT WorkArea { get; set; }
    public int Width => Bounds.Right - Bounds.Left;
    public int Height => Bounds.Bottom - Bounds.Top;
    public bool IsPrimary { get; set; }
    public int Index { get; set; }
}

[StructLayout(LayoutKind.Sequential)]
public struct OSMONITORINFO
{
    public uint cbSize;
    public RECT rcMonitor;
    public RECT rcWork;
    public uint dwFlags;
}