using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    public partial class ActionQueueTriggerConfig : UserControl, ITriggerConfig
    {
        private readonly ActionQueueTrigger _actionQueueTrigger;
        private bool _initComplete = false;
        public ActionQueueTriggerConfig(ActionQueueTrigger actionQueueTrigger)
        {
            _actionQueueTrigger = actionQueueTrigger;
            InitializeComponent();
            NotificationDelayInput.Value = (decimal)GeneralHelper.ConstrainValue<double>(_actionQueueTrigger.NotificationDelay, 0, 1000);
            _initComplete = true;
        }

        public UserControl ControlHandle { get { return this; } }

        private void NotificationDelayInput_ValueChanged(object sender, EventArgs e)
        {
            _actionQueueTrigger.NotificationDelay = (double)NotificationDelayInput.Value;
        }

        private void ModifyConditionsBtn_Click(object sender, EventArgs e)
        {
            LogQueueParseHelper.EditModFile();
        }
    }
}
