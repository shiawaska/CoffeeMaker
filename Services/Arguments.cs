using StartupScriptApp.Enums;
using StartupScriptApp.Interfaces;
using StartupScriptApp.Models.Configurations;

namespace StartupScriptApp.Services;

public class Arguments(ILogger logger)
{
    public readonly Dictionary<string, List<string>> argDict =
        new();
    
    private bool _debug;
    private bool _verbose;
    public bool debug => argDict.ContainsKey("debug");
    public bool verbose => argDict.ContainsKey("verbose") || argDict.ContainsKey("v");

    private readonly HashSet<string> ValidFlags = new ()
    {
        "--seq", // Sequential startup
        "--help", // Show help
        "--h", // Show help (short)
        "--apps", // Show app list and groups
        "--verbose", // Verbose
        "--v", // Verbose
        "--use-defined-monitors", // Use defined monitor layout instead of OS-detected
        "--noSnap", // Don't snap windows after starting
        "--debug", // Enable debug output (e.g. window titles and handles)
        "--ext-search", // Enable extended search for window titles (useful for apps that spawn child processes for their windows)
        "--delaystart", // Delay start of all apps by specified ms (e.g. to wait for windows to finish its other startup actions)
        
    };

    private readonly HashSet<string> ValidArgsWithValues = new ()
    {
        "--group", // Can take multiple groups (IDEs, Coms, Utils, Doc)
        "--start", // Can take multiple apps to start
        "--skip", // Can take multiple apps to skip
        "--noretry", // Can take multiple apps (or empty for all)
        "--nocheck", // Can take multiple apps (or empty for all)
        "--min", // Can take multiple apps to minimize (or empty for all)
        "--max", // Can take multiple apps to maximize (or empty for all)
        "--monitor", // Can take multiple apps to that monitor (or empty for all)
        "--process", // Window snap: process name
        "--snap", // Window snap: snap position
        "--path", // use alternate path for the config file
    };

    public void SetArgs(string[] args)
    {
        argDict.Clear();
        BuildArgDict(args);

        if (argDict.ContainsKey("help") || argDict.ContainsKey("h"))
        {
            PrintHelp();
            Environment.Exit(0);
        }

        if (argDict.ContainsKey("apps"))
        {
            var verbose = argDict.ContainsKey("verbose") || argDict.ContainsKey("v");
            PrintApps(verbose);
            Environment.Exit(0);
        }

        if (debug)
        {
            logger.LogDebug("\nParsed Arguments:");
            foreach (var kvp in argDict)
            {
                logger.LogDebug($"\n  {kvp.Key}: {string.Join(", ", kvp.Value)}");
            }
        }
    }

    private void BuildArgDict(string[] args)
    {
        string? currentKey = null;
        foreach (var arg in args)
        {
            var argLower = arg.ToLower();

            if (argLower.StartsWith("--"))
            {
                if (!ValidFlags.Contains(argLower) && !ValidArgsWithValues.Contains(argLower))
                {
                    logger.LogInfo($"\nUnknown argument: {arg}");
                    logger.LogInfo(
                        $"debug @@ \nValid options: {string.Join(", ", ValidFlags.Concat(ValidArgsWithValues))}"
                    );
                    Environment.Exit(1);
                }

                currentKey = argLower.TrimStart('-');
                if (!argDict.ContainsKey(currentKey))
                    argDict[currentKey] = new List<string>();
            }
            else if (currentKey != null)
            {
                argDict[currentKey].Add(arg);
            }
        }
    }

    public bool HasFlag(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;
        var normalized = key.TrimStart('-').ToLowerInvariant();
        return argDict.ContainsKey(normalized);
    }
    public static bool HasFlag(string key, Arguments arguments)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;
        var normalized = key.TrimStart('-').ToLowerInvariant();
        return arguments.argDict.ContainsKey(normalized);
    }

    private void PrintHelp()
    {
        var groups = string.Join(", ", Enum.GetNames(typeof(Categories)));

        logger.LogInfo(
            @"
            StartupScript - Application Launcher

            USAGE:
                StartupScript [OPTIONS]

            OPTIONS:
                --help, --h                      Show this help message
                --apps                           List available applications and groups
                --v, --verbose                   Verbose output (e.g. show app details in --apps)
                --debug                          Enable debug output
                --seq                            Start applications sequentially (default: parallel)

                --group <Group> [<Group>...]     Start only apps in specified groups
                                                 Groups: "
                + groups
                + @"
                --start <App> [<App>...]         Start only specified apps
                --skip <App> [<App>...]          Skip specified apps

                --nocheck [<App>...]             Skip running-process check
                                                 No apps = apply to all
                --noretry [<App>...]             Do not retry failed starts
                                                 No apps = apply to all

                --min [<App>...]                 Minimize after starting
                                                 No apps = apply to all
                --max [<App>...]                 Maximize after starting
                                                 No apps = apply to all

                --monitor <N>                    Override default monitor for specified apps
                                                 No apps = apply to all
                --use-defined-monitors           Use defined monitor layout instead of OS layout
                --nosnap                         Do not snap windows after starting

            EXAMPLES:
                StartupScript --seq
                StartupScript --group IDEs Communications
                StartupScript --start Outlook Teams Chrome
                StartupScript --start Outlook Teams --max Outlook --min Teams
                StartupScript --skip Chrome Teams
                StartupScript --start Outlook --max --monitor 2
                StartupScript --min --start StreamDeck OpenVPN PowerToys
                StartupScript --group Utilities --nocheck --noretry
            "
        );
    }

    private void PrintApps(bool verbose = false)
    {
        logger.LogInfo("Available Applications:");
        foreach (var app in ConfigurationsDefaults.Applications)
        {
            if (app.IsActive)
            {
                if (verbose)
                {
                    logger.LogInfo(
                        $"\n{app.Name}\n"
                            + $"  Category        : {app.Category}\n"
                            + $"  Executable      : {app.ExecutablePath}\n"
                            + $"  Arguments       : {app.Arguments ?? "(none)"}\n"
                            + $"  Process Name    : {app.ProcessName}\n"
                            + $"  Working Dir     : {app.WorkingDirectory ?? "(none)"}\n"
                            + $"  Order           : {app.Order}\n"
                            + $"  Monitor         : {app.MonitorIndex}\n"
                            + $"  Position        : {app.Position}\n"
                            + $"  Window Style    : {app.WindowStyle}\n"
                    );
                }
                else
                    logger.LogInfo($"\n - {app.Name} \nCategory: {app.Category}");
            }
        }
        Environment.Exit(0);
    }
}