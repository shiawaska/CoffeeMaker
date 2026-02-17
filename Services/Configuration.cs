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
            logger.LogError("Failed to build application definitions from configuration. Exiting.");
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
}
