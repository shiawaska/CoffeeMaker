using StartupScriptApp.Models.Configurations;

namespace StartupScriptApp.Interfaces;

public interface IConfigurationService
{
    Task<AppConfigFile> LoadConfigurationAsync(string filePath);
    void BuildApplicationDefinitions(List<ApplicationDefinitionDto> definitions);
    void ApplyDefaults(ConfigDefaults defaults);
    void InitializeFromJson(string filePath);
}