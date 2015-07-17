using System;

namespace Aldurcraft.Persistent40
{
    public interface IPersistentLogger
    {
        void LogDebug(string message, object source, Exception exception = null);
        void LogInfo(string message, object source, Exception exception = null);
        void LogError(string message, object source, Exception exception = null);
    }
}
