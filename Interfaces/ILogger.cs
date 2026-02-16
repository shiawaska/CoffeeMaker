using StartupScriptApp.Enums;
using StartupScriptApp.Models;
using StartupScriptApp.Services;

namespace StartupScriptApp.Interfaces;

public interface ILogger
{
    public void SetArguments(Arguments arguments);
    public void LogMulti(string input, params LogLevel[] levels);
    public void LogDebug(string input);
    public void LogVerbose(string input); 
    public void LogInfo(string input);
    public void LogError();
    public void LogError(string input); 
    public void LogError(Exception ex, string? message = null);
    public void PrintApps();
    public void PrintDefaults();
    public void PrintMonitors(List<MonitorInfo> monitors);
}