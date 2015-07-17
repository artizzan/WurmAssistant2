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
    public partial class MeditationTimerOptions : Form
    {
        MeditationTimer MedTimer;
        FormTimerSettingsDefault parentForm;
        bool inited = false;
        public MeditationTimerOptions(MeditationTimer timer, FormTimerSettingsDefault parent)
        {
            MedTimer = timer;
            parentForm = parent;
            InitializeComponent();
            checkBoxRemindSleepBonus.Checked = MedTimer.SleepBonusReminder;
            int popupDurationMillis = MedTimer.SleepBonusPopupDuration;
            this.numericUpDownPopupDuration.Value = GeneralHelper.ConstrainValue<int>(
                popupDurationMillis / 1000,
                (int)numericUpDownPopupDuration.Minimum,
                (int)numericUpDownPopupDuration.Maximum);
            checkBoxShowMeditSkill.Checked = MedTimer.ShowMeditSkill;
            checkBoxCount.Checked = MedTimer.ShowMeditCount;
            inited = true;
        }

        private void MeditationTimerOptions_Load(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                this.Location = FormHelper.GetCenteredChildPositionRelativeToParentWorkAreaBound(this, parentForm);
                UpdateSleepBonusPanelVisibility();
            }
            toolTip1.SetToolTip(checkBoxRemindSleepBonus, "If sleep bonus was turned on just before meditation,\r\nthis will pop a reminder once it can be turned off");
        }

        void UpdateSleepBonusPanelVisibility()
        {
            if (checkBoxRemindSleepBonus.Checked) panelPopupDuration.Visible = true;
            else panelPopupDuration.Visible = false;
        }

        private void checkBoxRemindSleepBonus_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSleepBonusPanelVisibility();
            if (inited) MedTimer.SleepBonusReminder = checkBoxRemindSleepBonus.Checked;
        }

        private void numericUpDownPopupDuration_ValueChanged(object sender, EventArgs e)
        {
            if (inited) MedTimer.SleepBonusPopupDuration = ((int)numericUpDownPopupDuration.Value) * 1000;
        }

        private void checkBoxShowMeditSkill_CheckedChanged(object sender, EventArgs e)
        {
            if (inited) MedTimer.ShowMeditSkill = checkBoxShowMeditSkill.Checked;
        }

        private void checkBoxCount_CheckedChanged(object sender, EventArgs e)
        {
            if (inited) MedTimer.ShowMeditCount = checkBoxCount.Checked;
        }
    }
}
