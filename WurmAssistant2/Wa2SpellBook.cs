using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2
{
    /// <summary>
    /// Source of all magic!
    /// </summary>
    public static class Wa2SpellBook
    {
        public static void ExecCatchLog(Action action, object source = null, string errorText = null, bool showAsInfoOnly = false)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                if (showAsInfoOnly)
                {
                    Logger.LogInfo(errorText, source, exception);
                }
                else
                {
                    Logger.LogError(errorText, source, exception);
                }
            }
        }
    }
}
