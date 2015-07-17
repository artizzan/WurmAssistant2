using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    public partial class SimpleConditionTriggerBaseConfig : UserControl, ITriggerConfig
    {
        private readonly SimpleConditionTriggerBase _simpleConditionTriggerBase;

        private bool _initComplete = false;
        public SimpleConditionTriggerBaseConfig(SimpleConditionTriggerBase simpleConditionTriggerBase)
        {
            _simpleConditionTriggerBase = simpleConditionTriggerBase;
            InitializeComponent();
            ConditionTbox.Text = simpleConditionTriggerBase.Condition;
            DescLabel.Text = simpleConditionTriggerBase.ConditionHelp;
            _initComplete = true;
        }

        public UserControl ControlHandle { get { return this; } }

        private void ConditionTbox_TextChanged(object sender, EventArgs e)
        {
            if (_initComplete) _simpleConditionTriggerBase.Condition = ConditionTbox.Text;
        }
    }
}
