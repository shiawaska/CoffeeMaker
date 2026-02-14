using System.Diagnostics;
using StartupScriptApp.Interfaces;

namespace StartupScriptApp.Services;

public class ProcessManagement(ILogger logger)
{
    private readonly ILogger _logger;
    public bool IsProcessRunning(string processName)
    {
        _logger.DebugLog($"Checking if process '{processName}' is running...");
        var isEmpty = string.IsNullOrWhiteSpace(processName);

        if (isEmpty)
        {
            _logger.DebugLog("Process name is empty or whitespace. Skipping running check.");
            return false;
        }
        bool running = Process.GetProcessesByName(processName).Length > 0;

        _logger.DebugLog($"Process '{processName}' running: {running}");
        return running;
    }

    public Process? StartProcess(ProcessStartInfo processStartInfo)
    {
        try
        {
            _logger.DebugLog(
                $"Starting process: {processStartInfo.FileName}, args: {processStartInfo.Arguments}, or working directory {processStartInfo.WorkingDirectory} with window style {processStartInfo.WindowStyle}, UseShellExecute={processStartInfo.UseShellExecute}"
            );
            _logger.DebugLog(
                $"ProcessStartInfo details: FileName='{processStartInfo.FileName}'"
            );
            return Process.Start(processStartInfo);
        }
        catch (Exception ex)
        {
            _logger.InfoLog($"\n {ex.Message}");
        }
        return null;
    }
}