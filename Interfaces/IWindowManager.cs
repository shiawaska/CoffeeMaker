using System.Diagnostics;
using StartupScriptApp.Enums;
using StartupScriptApp.Models;
using StartupScriptApp.Models.ApplicationDefinition;

namespace StartupScriptApp.Interfaces;

public interface IWindowManager
{
    public Task<List<IntPtr>> FindWindowAsync(ApplicationDefinition app);
    public Task<List<IntPtr>> FindWindowAsync(ApplicationDefinition app, Process proc);
    public bool SetWindowPosition(IntPtr hWnd, SnapPosition snapPosition, MonitorInfo monitor);
}
