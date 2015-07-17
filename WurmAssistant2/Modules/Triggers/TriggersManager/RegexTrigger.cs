using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Aldurcraft.Utility.MessageSystem;
using Aldurcraft.Utility.Notifier;
using Aldurcraft.Utility.PopupNotify;
using Aldurcraft.Utility.SoundEngine;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    [DataContract]
    public class RegexTrigger : SimpleConditionTriggerBase
    {
        public RegexTrigger() : base()
        {
            Init();
        }

        protected override bool CheckCondition(string logMessage)
        {
            if (string.IsNullOrEmpty(Condition)) return false;
            try
            {
                return Regex.IsMatch(logMessage, Condition);
            }
            catch (Exception exception)
            {
                Logger.LogError(string.Format("Exception while checking regex trigger condition. Trigger name: {0}, Condition: {1}", Name, Condition), this, exception);
                return false;
            }
        }

        public override string TypeAspect
        {
            get { return "Regex"; }
        }

        public override IEnumerable<ITriggerConfig> Configs
        {
            get { return new List<ITriggerConfig>(base.Configs) { new RegexTriggerConfig(this) }; }
        }

        private void Init()
        {
            Condition = string.Empty;
            ConditionHelp = "Use C# regular expression pattern";
        }

        [OnDeserializing]
        private void OnDes(StreamingContext context)
        {
            Init();
        }
    }
}
