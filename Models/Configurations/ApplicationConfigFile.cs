namespace StartupScriptApp.Models.Configurations;

public class AppConfigFile
{
    public ConfigDefaults? Defaults { get; set; }
    public List<ApplicationDefinitionDto> Applications { get; set; } = new();
    public List<MonitorInfo> Monitors { get; set; } = new();
}