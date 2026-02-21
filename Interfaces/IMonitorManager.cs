using StartupScriptApp.Models;

namespace StartupScriptApp.Interfaces;

public interface IMonitorManager
{
    public List<MonitorInfo> GetAllMonitors();
    
}
