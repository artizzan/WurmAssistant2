using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility;
using Aldurcraft.Utility.SoundEngine;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Timers
{
    public partial class FormTimerSettingsDefault : Form
    {
        WurmTimer ParentTimer;
        bool isInited = false;
        public FormTimerSettingsDefault(WurmTimer wurmTimer)
        {
            ParentTimer = wurmTimer;
            InitializeComponent();
            if (wurmTimer.MoreOptionsAvailable) buttonMoreOptions.Visible = true;
            this.Text = wurmTimer.TimerID;
            //set all options values
            this.checkBoxPopup.Checked = ParentTimer.PopupNotify;
            this.checkBoxSound.Checked = ParentTimer.SoundNotify;
            this.checkBoxPopupPersistent.Checked = ParentTimer.PersistentPopup;
            this.textBoxSoundName.Text = ParentTimer.SoundName;
            this.checkBoxOnAssistantLaunch.Checked = ParentTimer.PopupOnWALaunch;
            int popupDurationMillis = ParentTimer.PopupDuration;
            this.numericUpDownPopupDuration.Value = GeneralHelper.ConstrainValue<int>(
                popupDurationMillis / 1000,
                (int)numericUpDownPopupDuration.Minimum,
                (int)numericUpDownPopupDuration.Maximum);
            isInited = true;
        }

        private void FormTimerSettingsDefault_Load(object sender, EventArgs e)
        {
            if (this.Visible) this.Location = FormHelper.GetCenteredChildPositionRelativeToParentWorkAreaBound(this, ParentTimer.GetModuleUI());
            this.toolTip1.SetToolTip(this.checkBoxPopupPersistent, "Popup must be closed manually");
            this.toolTip1.SetToolTip(this.buttonTurnOff, "Turn off this timer (your settings will be preserved)");
        }

        void UpdatePanels()
        {
            if (checkBoxPopup.Checked) panelPopup.Visible = true;
            else panelPopup.Visible = false;
            if (checkBoxSound.Checked) panelSoundNotify.Visible = true;
            else panelSoundNotify.Visible = false;
        }

        private void buttonMoreOptions_Click(object sender, EventArgs e)
        {
            ParentTimer.OpenMoreOptions(this);
        }

        private void checkBoxPopup_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePanels();
            if (isInited)
            {
                ParentTimer.PopupNotify = checkBoxPopup.Checked;
            }
        }

        private void checkBoxSound_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePanels();
            if (isInited)
            {
                ParentTimer.SoundNotify = checkBoxSound.Checked;
            }
        }

        private void checkBoxPopupPersistent_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxPopupPersistent.Checked)
            {
                numericUpDownPopupDuration.Enabled = false;
            }
            else
            {
                numericUpDownPopupDuration.Enabled = true;
            }

            if (isInited)
            {
                ParentTimer.PersistentPopup = checkBoxPopupPersistent.Checked;
            }
        }

        private void buttonSoundChange_Click(object sender, EventArgs e)
        {
            if (isInited)
            {
                string newsound = SoundBank.ChooseSound();
                if (newsound != null)
                {
                    textBoxSoundName.Text = newsound;
                    ParentTimer.SoundName = newsound;
                }
            }
        }

        private void buttonTurnOff_Click(object sender, EventArgs e)
        {
            ParentTimer.TurnOff();
            this.Close();
        }

        private void checkBoxOnAssistantLaunch_CheckedChanged(object sender, EventArgs e)
        {
            if (isInited)
            {
                ParentTimer.PopupOnWALaunch = checkBoxOnAssistantLaunch.Checked;
            }
        }

        private void numericUpDownPopupDuration_ValueChanged(object sender, EventArgs e)
        {
            if (isInited)
            {
                ParentTimer.PopupDuration = ((int)numericUpDownPopupDuration.Value) * 1000;
            }
        }
    }
}
