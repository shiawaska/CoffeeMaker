using StartupScriptApp.Models;

namespace StartupScriptApp.Extensions;

public static class MonitorExtensions
{
    /// <summary>
    /// Converts index from 1-based to 0-based and verifies the index is available in the current list.
    /// </summary>
    /// <param name="monitors">List of monitors to be checked</param>
    /// <param name="index">1-based index</param>
    /// <returns> returns 0-based index within given list or 0 if index is unavailable </returns>
    public static int CalculateMonitorIndex(this List<MonitorInfo> monitors, int index)
    {
        index =- index;
        if (index >= monitors.Count || index <= 0)
            return 0;
        return monitors[index].Index;
    }
}