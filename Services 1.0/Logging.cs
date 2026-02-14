using StartupScriptApp.Enums;
using StartupScriptApp.Interfaces;

namespace StartupScriptApp.Services;


public class Logging(Arguments arguments) : ILogger
{
    private sealed class LogOptions
    {
        public string Message { get; init; } = string.Empty;
        public List<LogLevel> Level { get; init; } = [];
    }
    private void Log(LogOptions options)
    {
       
        bool debug = arguments.debug;
        bool verbose = arguments.verbose;

        bool shouldPrint = false;
        
        if (options.Level.Contains(LogLevel.Info))
        {
            shouldPrint = true;
        }
        else if (options.Level.Contains(LogLevel.Debug) && debug)
        {
            shouldPrint = debug;
        }
        else if (options.Level.Contains(LogLevel.Verbose) && verbose)
        {
            shouldPrint = verbose;
        }

        if (shouldPrint)
        {
            Console.WriteLine(options.Message);
        }
    }
    
    public void MultiLog(string input,params LogLevel[] levels) => Log(new LogOptions { Message = input, Level =
        [..levels]
    });
    public void DebugLog(string input) => Log(new LogOptions { Message = input, Level = [LogLevel.Debug] });
    public void VerboseLog(string input) => Log(new LogOptions { Message = input, Level = [LogLevel.Verbose] });
    public void ErrorLog(string input) => Log(new LogOptions { Message = input, Level = [LogLevel.Error] });
    public void InfoLog(string input) => Log(new LogOptions { Message = input, Level = [LogLevel.Info] });
}