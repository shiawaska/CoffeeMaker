using StartupScriptApp.models.ApplicationDefinition;

namespace StartupScriptApp.Models.Configurations;

public static class ConfigurationsDefaults
{
    public static int WindowCaptureDelayMs { get; set; } = 3000;

    public static int ProcessCheckRetries { get; set; } = 3;

    public static int DelayBeforeSnapMs { get; set; } = 10000;
    public static List<ApplicationDefinition> Applications { get; set; } = new();
    public static List<MonitorConfig> Monitors { get; set; } = new();
}