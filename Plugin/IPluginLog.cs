
namespace Plugin
{
    public interface IPluginLog
    {
        void LogPluginCritical(string message);
        void LogPluginInformation(string message);
        void LogPluginError(string message);
        void LogPluginWarning(string message);
    }
}
