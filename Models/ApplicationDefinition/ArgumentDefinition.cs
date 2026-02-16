namespace StartupScriptApp.Models;

public class ArgumentDefinition(string name, string value)
{
        public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
        public string Value { get; } = value ?? throw new ArgumentNullException(nameof(value));
        public bool IncludeNameInWindowTitles { get; set; }

}