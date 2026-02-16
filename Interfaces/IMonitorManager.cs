using StartupScriptApp.Models;

namespace StartupScriptApp.Interop.Interfaces;

public interface IMonitorManager
{
    public List<MonitorInfo> GetAllMonitors();
    
}
