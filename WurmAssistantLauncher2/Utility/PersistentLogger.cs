using System;
using Aldurcraft.Persistent40;

namespace Aldurcraft.WurmAssistantLauncher2.Utility
{
    class PersistentLogger : IPersistentLogger
    {
        public void LogDebug(string message, object source, Exception exception = null)
        {
            App.Logger.LogDebug(message, source, exception);
        }

        public void LogInfo(string message, object source, Exception exception = null)
        {
            App.Logger.LogInfo(message, source, exception);
        }

        public void LogError(string message, object source, Exception exception = null)
        {
            App.Logger.LogError(message, source, exception);
        }
    }
}
