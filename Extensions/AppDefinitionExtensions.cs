using System.Diagnostics;
using StartupScriptApp.Models;
using StartupScriptApp.Models.ApplicationDefinition;

namespace StartupScriptApp.Extensions;

public static class AppDefinitionExtensions
{
     /// <summary>
    /// Checks if the app's name is included in the specified argument list (e.g., "start", "skip", "nocheck"). The check is case-insensitive. If the argument key is not present in the dictionary, this returns false.
    /// </summary>
    /// <param name="appName">The name of the app to check for in the argument list.</param>
    /// <param name="argDict">The dictionary of command-line arguments and their values.</param>
    /// <param name="argKey">The key of the argument list to check within the dictionary.</param>
    /// <returns>True if the app name is found in the specified argument list; otherwise false.</returns>
    private static bool IsAppInArgList(this
        string appName,
        Dictionary<string, List<string>> argDict,
        string argKey
    )
    {
        return argDict.ContainsKey(argKey)
            && argDict[argKey].Contains(appName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the app should be processed for startup based on the presence of its name in the "skip" argument list, the "group" argument list, and the "start" argument list. The app will be processed if it's not in the "skip" list and (the "start" list is empty or contains the app, or the app belongs to a group specified in the "group" list). If no "start" or "group" arguments are provided, all apps not in "skip" will be processed.
    /// </summary>
    /// <param name="app">The startup app to check for processing eligibility.</param>
    /// <param name="argDict">The dictionary of command-line arguments and their values.</param>
    /// <returns>True if the app should be processed for startup; otherwise false.</returns>
    public static bool ShouldProcessApp(this ApplicationDefinition app, Dictionary<string, List<string>> argDict)
    {
        var inSkipList = !String.IsNullOrWhiteSpace(app.Name) && app.Name.IsAppInArgList(argDict, "skip");
        var inStartList = !String.IsNullOrWhiteSpace(app.Name) && app.Name.IsAppInArgList(argDict, "start");
        var inGroup = app.IsAppInGroup(argDict);

        return !inSkipList
            && (
                IsDefaultStartAll(argDict)
                || inGroup
                || inStartList
            );
    }

    /// <summary>
    ///  Determines whether to skip the running process check for the app based on the "nocheck" argument. If "nocheck" is not present, this returns false (don't skip, i.e. check if running). If "nocheck" is present with no specific apps, this returns true (skip for all). If "nocheck" includes specific apps, this returns true for those apps and false for others.
    /// </summary>
    /// <param name="app">The startup app to check for process check skipping.</param>
    /// <param name="argDict">The dictionary of command-line arguments and their values.</param>
    /// <returns>True if the process check should be skipped for the app; otherwise false.</returns>
    public static bool ShouldSkipProcessCheck(
       this ApplicationDefinition app,
        Dictionary<string, List<string>> argDict
    )
    {
        return  app.SkipRunningCheck || string.IsNullOrWhiteSpace(app.ProcessName)
            || (
                argDict.ContainsKey("nocheck")
                && (!argDict["nocheck"].Any() || app.Name.IsAppInArgList(argDict, "nocheck"))
            );
    }

    /// <summary>
    /// Determines whether the app should be retried on failure based on the "noretry" argument. If "noretry" is not present, this returns true (retry enabled). If "noretry" is present with no specific apps, this returns false (retry disabled for all). If "noretry" includes specific apps, this returns false for those apps and true for others.
    /// </summary>
    /// <param name="app">The startup app to check for retry eligibility.</param>
    /// <param name="argDict">The dictionary of command-line arguments and their values.</param>
    /// <returns>True if the app should be retried on failure; otherwise false.</returns>
    public static bool ShouldRetry(this ApplicationDefinition app, Dictionary<string, List<string>> argDict)
    {
        return !argDict.ContainsKey("noretry")
            || (!app.Name.IsAppInArgList(argDict, "noretry") && argDict["noretry"].Any());
    }

    /// <summary>
    /// Determines the window style to use when starting the app based on command-line arguments. If the app is included in the "min" argument list (or if "min" is present with no specific apps), this returns Minimized. If included in the "max" argument list (or "max" with no specific apps), it returns Maximized. Otherwise, it returns the app's default WindowStyle.
    /// </summary>
    /// <param name="app">The startup app for which to determine the window style.</param>
    /// <param name="argDict">The dictionary of command-line arguments and their values.</param>
    /// <returns>The resolved window style to apply when starting the app.</returns>
    public static ProcessWindowStyle GetWindowStyle(
       this ApplicationDefinition app,
        Dictionary<string, List<string>> argDict
    )
    {
        if (
            argDict.ContainsKey("min")
            && (!argDict["min"].Any() || app.Name.IsAppInArgList(argDict, "min"))
        )
            return ProcessWindowStyle.Minimized;

        if (
            argDict.ContainsKey("max")
            && (!argDict["max"].Any() || app.Name.IsAppInArgList(argDict, "max"))
        )
            return ProcessWindowStyle.Maximized;

        return app.WindowStyle;
    }

    /// <summary>
    /// Determines whether the app belongs to any of the groups specified in the "group" argument list. If no "group" argument is provided, this returns false, meaning group filtering is not applied.
    /// </summary>
    /// <param name="app">The startup app to check.</param>
    /// <param name="argDict">The dictionary of command-line arguments and their values.</param>
    /// <returns>True when the app belongs to any of the specified groups; otherwise false.</returns>
    private static bool IsAppInGroup(this ApplicationDefinition app, Dictionary<string, List<string>> argDict)
    {
        if (!argDict.TryGetValue("group", out var requestedGroups))
            return false;

        return requestedGroups.Any(group =>
            string.Equals(group, app.Category.ToString(), StringComparison.OrdinalIgnoreCase)
        );
    }

    /// <summary>
    /// Determines if the default behavior of starting all apps should be applied, which is when no specific start or group arguments are provided.
    /// </summary>
    /// <param name="argDict">The dictionary of command-line arguments and their values.</param>
    /// <returns>True when default start-all behavior applies; otherwise false.</returns>
    private static bool IsDefaultStartAll(Dictionary<string, List<string>> argDict)
    {
        return !argDict.ContainsKey("group") && !argDict.ContainsKey("start");
    }
}