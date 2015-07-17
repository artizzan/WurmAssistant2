using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger
{
    class GrangerDebugLogger
    {
        public void Log(string message)
        {
            Logger.LogInfo(message, "Granger debug");
        }

        internal void Log(string message, bool isError, Exception _e = null)
        {
            if (_e == null) Logger.LogError(message, "Granger debug");
            else Logger.LogError(message, "Granger debug", _e);
        }
    }
}
