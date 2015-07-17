using System;

namespace Aldurcraft.Persistent40
{
    class DummyLogger : IPersistentLogger
    {
        public void LogDebug(string message, object source, Exception exception = null)
        {
            //quack!
        }

        public void LogInfo(string message, object source, Exception exception = null)
        {
            //woff!!!
        }

        public void LogError(string message, object source, Exception exception = null)
        {
            //exterminate exterminate
        }
    }
}
