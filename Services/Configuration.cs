using System.Reflection;
using System.Text.Json;
using StartupScriptApp.Enums;
using StartupScriptApp.Interfaces;
using StartupScriptApp.Models;
using StartupScriptApp.Models.ApplicationDefinition;
using StartupScriptApp.Models.Configurations;

namespace StartupScriptApp.Services;

public class Configuration(ILogger logger) : IConfigurationService
{
    public async Task<AppConfigFile> LoadConfigurationAsync(string filePath)
    {
        await TestConfigAvailability(filePath);
        var json = string.Empty;
        try
        {
            logger.LogDebug($"Attempting to load configuration file: {filePath}", [Area.Config]);

            json = await File.ReadAllTextAsync(filePath);
            logger.LogDebug($"Loaded configuration file: {filePath}", [Area.Config]);
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to load configuration file. Exiting.");
            logger.LogError(ex, $"Failed to load configuration file: {filePath}");
            Environment.Exit(2);
        }

        AppConfigFile? config = null;
        try
        {
            config = JsonSerializer.Deserialize<AppConfigFile>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowOutOfOrderMetadataProperties = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                }
            );
            if (config == null)
            {
                throw new InvalidOperationException("Failed to deserialize configuration file");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                "Exception occured during configuration file deserialization. Exiting."
            );
            logger.LogError(
                ex,
                $"\nException occured during configuration file deserialization: \n {filePath}"
            );

            Environment.Exit(3);
        }

        return config;
    }

    public void BuildApplicationDefinitions(List<ApplicationDefinitionDto> definitions)
    {
        logger.LogDebug("Building application definitions from configuration", [Area.Config]);
        LogAppsWithoutCategory(definitions);

        try
        {
            var builders = definitions.Select(dto => (ApplicationDefinition.Builder)dto).ToList();
            ConfigurationsDefaults.Applications = builders
                .Select(b => b.Build())
                .Where(a => a.IsActive)
                .OrderBy(a => a.Order)
                .ToList();
            logger.PrintApps(ConfigurationsDefaults.Applications, "Applications Loaded :");
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to build application definitions from configuration. Exiting.",true);
            logger.LogError(ex, "Failed to build application definitions from configuration.");
            Environment.Exit(1);
        }
    }

    public void ApplyDefaults(ConfigDefaults? defaults)
    {
        logger.LogDebug("Applying default configuration values", [Area.Config]);
        ConfigurationsDefaults.WindowCaptureDelayMs =
            defaults?.WindowCaptureDelayMs ?? ConfigurationsDefaults.WindowCaptureDelayMs;
        ConfigurationsDefaults.WindowCaptureRetries =
            defaults?.WindowCaptureRetries ?? ConfigurationsDefaults.WindowCaptureRetries;
        ConfigurationsDefaults.DelayBeforeSnapMs =
            defaults?.DelayBeforeSnapMs ?? ConfigurationsDefaults.DelayBeforeSnapMs;
        logger.PrintDefaults();
    }

    public void InitializeFromJson(string filePath)
    {
        var file = LoadConfigurationAsync(filePath).Result;
        BuildApplicationDefinitions(file.Applications);
        ApplyDefaults(file.Defaults);
        LoadMonitorConfig(file.Monitors);
    }

    private async Task TestConfigAvailability(string filePath)
    {
        if (!File.Exists(filePath))
        {
            logger.LogInfo("No config file found. Creating default config file.");
            logger.LogInfo("Would you like a template config file? (y/n)");
            var response = Console.ReadLine();
            if (response?.ToLower() != "n") return;
            
            await using var resourceStream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream("StartupScriptApp.Resources.CoffeeMakerConfig.json");
            if (resourceStream == null)
                throw new FileNotFoundException("Resource file not found");

            var configFolder = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(configFolder))
                Directory.CreateDirectory(configFolder!);

            await using var fileStream = File.Create(filePath);

            await resourceStream.CopyToAsync(fileStream);
            logger.LogInfo("Created default config file at path : " + filePath);
            Environment.Exit(0);
        }
    }

    private void LoadMonitorConfig(List<MonitorInfo> monitors)
    {
        logger.LogDebug("Loading monitor configuration", [Area.Config]);
        ConfigurationsDefaults.Monitors = monitors;
        logger.PrintMonitors("Monitors Loaded from config file ", ConfigurationsDefaults.Monitors);
    }
    private void LogAppsWithoutCategory(List<ApplicationDefinitionDto> definitions)
    {
        var apps = definitions.Where(a => string.IsNullOrEmpty(a.Category)).ToList();
        logger.LogDebug("Applications without category:", [Area.Config],true);
        foreach (var app in apps)
        {
            logger.LogDebug($" - {app.Name}", [Area.Config],true);
            logger.LogDebug($"   Category: {app.Category}", [Area.Config],true);
            logger.LogDebug($"   Executable: {app.ExecutablePath}", [Area.Config],true);
            logger.LogDebug($"   Arguments: {app.Arguments}", [Area.Config],true);
            logger.LogDebug($"   Working Dir: {app.WorkingDirectory}", [Area.Config],true);
            logger.LogDebug($"   Order: {app.Order}", [Area.Config],true);
            logger.LogDebug($"   Process Name: {app.ProcessName}", [Area.Config],true);
            logger.LogDebug($"   Is Active: {app.IsActive}", [Area.Config],true);
            logger.LogDebug($"   Skip Running Check: {app.SkipRunningCheck}", [Area.Config],true);
            logger.LogDebug($"   Monitor Index: {app.MonitorIndex}", [Area.Config],true);
            logger.LogDebug($"   Position: {app.Position}", [Area.Config],true);
            logger.LogDebug($"   Verb: {app.Verb}", [Area.Config],true);
            logger.LogDebug($"   Use Shell Execute: {app.UseShellExecute}", [Area.Config],true);
            logger.LogDebug($"   Create No Window: {app.CreateNoWindow}", [Area.Config],true);
            logger.LogDebug($"   Splash Titles: {string.Join(", ", app.SplashTitles)}", [Area.Config],true);
            
        }
    }
    
    
}
