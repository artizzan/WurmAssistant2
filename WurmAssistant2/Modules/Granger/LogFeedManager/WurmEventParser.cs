using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger
{
    [Obsolete]
    /// <summary>
    /// Moved to GrangerHelpers
    /// </summary>
    public static class WurmEventParser
    {
        public static HorseTrait[] GetTraitsFromLine(string line)
        {
            return GrangerHelpers.GetTraitsFromLine(line);
        }
    }
}
