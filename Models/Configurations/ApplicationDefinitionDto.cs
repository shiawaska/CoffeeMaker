namespace StartupScriptApp.Models.Configurations;

public class ApplicationDefinitionDto
{
    public string? Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string? WorkingDirectory { get; set; }
    public List<string> Arguments { get; set; } = new();
    public string? ProcessName { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; } = true;
    public bool SkipRunningCheck { get; set; }
    public int MonitorIndex { get; set; }
    public string Position { get; set; } = "fullscreen";
    public string? Verb { get; set; }
    public bool UseShellExecute { get; set; } = true;
    public bool CreateNoWindow { get; set; }
    public string WindowStyle { get; set; } = "Normal";
    public List<string> WindowTitles { get; set; } = new();
    public List<string> SplashTitles { get; set; } = new();
}