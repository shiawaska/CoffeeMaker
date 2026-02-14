namespace StartupScriptApp.Models;

public class MonitorConfig
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsPrimary { get; set; }
    public double ScaleFactor { get; set; } = 1.0;
    public int RefreshRate { get; set; } = 60;
}
