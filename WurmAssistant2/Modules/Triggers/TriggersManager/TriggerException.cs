using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    public class TriggerException : Exception
    {
        public TriggerException(string message) : base(message) {}
    }
}
