using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Aldurcraft.Spellbook40.WizardTower
{
    public enum LogSeverity
    {
        Debug, Info, Error
    }

    public static class SpellbookLogger
    {
        public static event EventHandler<SpellbookLoggerEventArgs> Logged;
        internal static void LogError(string message, object source, Exception exception = null)
        {
            OnLogged(message, source, exception, LogSeverity.Error);
        }

        static void OnLogged(string message, object source, Exception exception, LogSeverity severity)
        {
            var eh = Logged;
            if (eh != null)
            {
                Logged("Spellbook40", 
                    new SpellbookLoggerEventArgs(message, source, exception, severity));
            }
        }

        public static void LogInfo(string message, object source, Exception exception = null)
        {
            OnLogged(message, source, exception, LogSeverity.Info);
        }

        [Conditional("DEBUG")]
        public static void LogDiag(string message, object source, Exception exception = null)
        {
            OnLogged(message, source, exception, LogSeverity.Debug);
        }
    }

    public class SpellbookLoggerEventArgs : EventArgs
    {
        public string Message { get; private set; }
        public object Source { get; private set; }
        public Exception Exception { get; private set; }
        public LogSeverity Severity { get; private set; }

        public SpellbookLoggerEventArgs(string message, object source, Exception exception, LogSeverity severity)
        {
            Message = message;
            Source = source;
            Exception = exception;
            Severity = severity;
        }
    }
    

}
