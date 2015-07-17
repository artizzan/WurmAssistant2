using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Aldurcraft.Utility.Helpers
{
    public class AppRun
    {
        public static bool RunLink(string link, string customError = null, object source = null)
        {
            try
            {
                Process.Start(link);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(customError ?? string.Format("Running following link failed: {0}", link), source, ex);
                return false;
            }
        }
    }
}
