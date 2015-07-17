using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Timers
{
    public partial class MeditPathTimerOptions : Form
    {
        private FormTimerSettingsDefault defaultSettingsForm;
        private MeditPathTimer meditPathTimer;

        public MeditPathTimerOptions(FormTimerSettingsDefault defaultSettingsForm, MeditPathTimer meditPathTimer)
        {
            this.defaultSettingsForm = defaultSettingsForm;
            this.meditPathTimer = meditPathTimer;
            InitializeComponent();
        }

        bool ManualCDSet = false;

        private void MeditPathTimerOptions_Load(object sender, EventArgs e)
        {
            if (this.Visible) this.Location = FormHelper.GetCenteredChildPositionRelativeToParentWorkAreaBound(this, defaultSettingsForm);
            if (meditPathTimer.NextQuestionAttemptOverridenUntil != DateTime.MinValue) ManualCDSet = true;
            RefreshButtonText();
        }

        private void buttonManualCD_Click(object sender, EventArgs e)
        {
            if (ManualCDSet)
            {
                meditPathTimer.NextQuestionAttemptOverridenUntil = DateTime.MinValue;
                ManualCDSet = false;
                RefreshButtonText();
                meditPathTimer.UpdateCooldown();
            }
            else
            {
                FormChooseQTimerManually ui = new FormChooseQTimerManually(meditPathTimer);
                if (ui.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    meditPathTimer.SetManualQTimer(ui.GetResultMeditLevel(), ui.GetResultOriginDate());
                    ManualCDSet = true;
                    RefreshButtonText();
                }
            }
        }

        void RefreshButtonText()
        {
            if (ManualCDSet) buttonManualCD.Text = "Manual cooldown is set\r\nClick to remove";
            else buttonManualCD.Text = "Set cooldown manually";
        }
    }
}
