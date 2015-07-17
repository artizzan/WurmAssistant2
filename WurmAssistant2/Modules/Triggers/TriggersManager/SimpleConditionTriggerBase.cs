using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    [DataContract]
    public abstract class SimpleConditionTriggerBase : TriggerBase
    {
        [DataMember]
        private string _condition;

        public string Condition
        {
            get { return _condition; }
            set { _condition = value; }
        }

        [OnDeserializing]
        void OnDes(StreamingContext context)
        {
            Init();
        }

        [OnDeserialized]
        void AfterDes(StreamingContext context)
        {
            if (_condition == null) _condition = string.Empty;
        }

        protected SimpleConditionTriggerBase() : base()
        {
            Init();
        }

        void Init()
        {
            _condition = string.Empty;
        }

        public string ConditionHelp { get; set; }

        public override string ConditionAspect
        {
            get { return Condition; }
        }

        public override IEnumerable<ITriggerConfig> Configs
        {
            get
            {
                return new List<ITriggerConfig>(base.Configs) { new SimpleConditionTriggerBaseConfig(this) };
            }
        }
    }
}
