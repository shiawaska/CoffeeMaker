using StartupScriptApp.Enums;
using StartupScriptApp.Models;
using StartupScriptApp.Models.ApplicationDefinition;
using StartupScriptApp.Services;

namespace StartupScriptApp.Interfaces;

public interface ILogger
{
    public void SetArguments(Arguments arguments);
    public void LogDebug(string input, bool? verboseGate = null);
    public void LogDebug(string input, Area[] areas, bool? verboseGate = null);
    public void LogInfo(string input, bool? verboseGate = null);
    public void LogError();
    public void LogError(string input, bool? verboseGate = null);
    public void LogError(Exception ex, string? message = null);
    public void PrintApps(List<ApplicationDefinition> apps, string? headerMessage = null);
    public void PrintDefaults();
    public void PrintMonitors(string headerMessage, List<MonitorInfo> monitors);
}