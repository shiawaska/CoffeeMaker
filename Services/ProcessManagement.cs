using System.Diagnostics;
using StartupScriptApp.Extensions;
using StartupScriptApp.Interfaces;
using StartupScriptApp.Models;
using StartupScriptApp.Models.ApplicationDefinition;

namespace StartupScriptApp.Services;

public class ProcessManagement(ILogger logger)
{
    public bool IsProcessRunning(string processName)
    {
        logger.LogDebug($"\n\n Checking if process '{processName}' is running...");
        var isEmpty = string.IsNullOrWhiteSpace(processName);

        if (isEmpty)
        {
            logger.LogDebug("Process name is empty or whitespace. Skipping running check.");
            return false;
        }
        bool running = Process.GetProcessesByName(processName).Length > 0;

        logger.LogDebug($"Process '{processName}' running: {running}");
        return running;
    }

    public Process? StartProcess(ProcessStartInfo processStartInfo)
    {
        try
        {
            logger.LogDebug(
                $"\n\n Starting process: {processStartInfo.FileName}, args: {processStartInfo.Arguments}, or working directory {processStartInfo.WorkingDirectory} with window style {processStartInfo.WindowStyle}, UseShellExecute={processStartInfo.UseShellExecute}"
            );
            logger.LogDebug(
                $"ProcessStartInfo details: FileName='{processStartInfo.FileName}'"
            );
            var process = Process.Start(processStartInfo);
            if (process != null)
            {
                // Wait for the process to start and get a valid MainWindowHandle if possible
                process.Refresh();
            }
            return process;
        }
        catch (Exception ex)
        {
            logger.LogInfo($"\n {ex.Message}");
        }

        return null;

    }
    
    public bool ProcessStartupChecklistAsync(ApplicationDefinition app ,Dictionary<string, List<string>> argDict)
    {
        bool shouldStart = false;
        bool runningCheck = false;
        bool isRunning = false;
        
        shouldStart = app.ShouldProcessApp(argDict);
        runningCheck = app.ShouldSkipProcessCheck(argDict);
        
        if (shouldStart && runningCheck)
        {
            isRunning = IsProcessRunning(app.ProcessName ?? string.Empty);
        }

        return shouldStart && (!runningCheck || !isRunning);
                
    }

    /// <summary>
    /// Attempts to find the main process (with a window) spawned by a starter process.
    /// </summary>
    /// <param name="starterProcess">The process returned by StartProcess.</param>
    /// <param name="processName">The expected main process name.</param>
    /// <param name="timeoutMs">How long to wait for the main process to appear.</param>
    /// <returns>The main process with a window, or null if not found.</returns>
    public Process? FindMainProcess(Process starterProcess, string processName, int timeoutMs = 5000)
    {
        if (string.IsNullOrWhiteSpace(processName))
        {
            logger.LogInfo("FindMainProcess: processName is null or empty. Cannot search for main process.");
            return null;
        }
        var startTime = starterProcess.StartTime;
        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            var candidates = Process.GetProcessesByName(processName)
                .Where(p => p.Id != starterProcess.Id && p.StartTime >= startTime)
                .ToList();
            foreach (var proc in candidates)
            {
                proc.Refresh();
                if (proc.MainWindowHandle != IntPtr.Zero)
                {
                    logger.LogDebug($"Found main process: {proc.ProcessName} (PID: {proc.Id}) with window handle {proc.MainWindowHandle}");
                    return proc;
                }
            }
            Thread.Sleep(500);
        }
        logger.LogInfo($"Could not find main process with window for '{processName}' after {timeoutMs}ms");
        return null;
    }   
}