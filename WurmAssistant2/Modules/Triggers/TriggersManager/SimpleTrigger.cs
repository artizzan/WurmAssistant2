using System;
using System.Ex;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Aldurcraft.Utility.MessageSystem;
using Aldurcraft.Utility.PopupNotify;
using Aldurcraft.Utility.SoundEngine;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    [DataContract]
    public class SimpleTrigger : SimpleConditionTriggerBase
    {
        public SimpleTrigger() : base()
        {
            Init();
        }

        protected override bool CheckCondition(string logMessage)
        {
            if (string.IsNullOrEmpty(Condition)) return false;
            return CheckCaseInsensitive(logMessage);
        }

        private bool CheckCaseInsensitive(string logMessage)
        {
            return logMessage.Contains(Condition, StringComparison.OrdinalIgnoreCase);
        }

        public override string TypeAspect
        {
            get { return "Simple"; }
        }

		private void Init()
		{
            ConditionHelp = "Text to find in logs, case insensitive";
		}

		[OnDeserializing]
		private void OnDes(StreamingContext context)
		{
			Init();
		}
    }
}
