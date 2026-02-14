namespace StartupScriptApp.Models.Configurations;

public class AppConfigurationFile
{
    public DefaultsConfiguration? Defaults { get; set; }
    public List<ApplicationDefinitionDto> Applications { get; set; } = new();
    public List<MonitorConfig> Monitors { get; set; } = new();
    
}


public class DefaultsConfiguration
{
    public int? WindowCaptureDelayMs { get; set; }
    public int? ProcessCheckRetries { get; set; }
    public int? DelayBeforeSnapMs { get; set; }
}

public class ApplicationDefinitionDto
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string? WorkingDirectory { get; set; }
    public string? Arguments { get; set; }
    public string? ProcessName { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; } = true;
    public bool SkipRunningCheck { get; set; }
    public int MonitorIndex { get; set; }
    public string Position { get; set; } = "None";
    public string? Verb { get; set; }
    public bool UseShellExecute { get; set; } = true;
    public bool CreateNoWindow { get; set; }
    public string WindowStyle { get; set; } = "Normal";
    public List<string> WindowTitles { get; set; } = new();
    public List<string> SplashTitles { get; set; } = new();
}