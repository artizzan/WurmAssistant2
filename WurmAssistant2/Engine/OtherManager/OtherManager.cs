using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2
{
    /// <summary>
    /// unfinished
    /// </summary>
    static class OtherManager
    {
        private static List<OtherTool> _otherTools = new List<OtherTool>();

        internal static void Init()
        {
            if (AssistantEngine.Settings.Value.OtherTools != null)
            {
                _otherTools.Clear();
                OtherTool[] savedTools = AssistantEngine.Settings.Value.OtherTools.OrderByDescending(x => x.SortOrder).ToArray();
                _otherTools.AddRange(savedTools);
            }
        }


    }
}
