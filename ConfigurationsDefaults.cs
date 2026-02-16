using StartupScriptApp.Models;
using StartupScriptApp.Models.ApplicationDefinition;

namespace StartupScriptApp;

public static class ConfigurationsDefaults
{
    public static readonly string DefaultConfigFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                                                          + "\\CoffeeMaker\\CoffeeMakerConfig.json";
    public static int WindowCaptureDelayMs { get; set; } = 3000;

    public static int WindowCaptureRetries { get; set; } = 3;

    public static int DelayBeforeSnapMs { get; set; } = 10000;
    public static List<ApplicationDefinition> Applications { get; set; } = new();
    public static List<MonitorInfo> Monitors { get; set; } = new();
}