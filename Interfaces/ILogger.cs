using StartupScriptApp.Enums;

namespace StartupScriptApp.Interfaces;

public interface ILogger
{
    public void MultiLog(string input, params LogLevel[] levels);
    public void DebugLog(string input);
    public void VerboseLog(string input); 
    public void ErrorLog(string input); 
    public void InfoLog(string input);
}