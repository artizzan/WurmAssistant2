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
    public partial class PrayerTimerOptions : Form
    {
        private PrayerTimer prayerTimer;
        private FormTimerSettingsDefault formSettings;

        public PrayerTimerOptions(PrayerTimer prayerTimer, FormTimerSettingsDefault form)
        {
            InitializeComponent();
            this.prayerTimer = prayerTimer;
            this.formSettings = form;

            numericUpDownFavorWhenThis.Value = 
                GeneralHelper.ConstrainValue<decimal>((decimal)prayerTimer.Settings.Value.FavorSettings.FavorNotifyOnLevel, 0M, 100M);
            checkBoxPopupPersist.Checked = prayerTimer.Settings.Value.FavorSettings.FavorNotifyPopupPersist;
            textBoxSoundName.Text = prayerTimer.Settings.Value.FavorSettings.FavorNotifySoundName;
            checkBoxNotifySound.Checked = prayerTimer.Settings.Value.FavorSettings.FavorNotifySound;
            checkBoxNotifyPopup.Checked = prayerTimer.Settings.Value.FavorSettings.FavorNotifyPopup;
            checkBoxFavorWhenMAX.Checked = prayerTimer.Settings.Value.FavorSettings.FavorNotifyWhenMAX;
            checkBoxShowFaithSkill.Checked = prayerTimer.ShowFaithSkillOnTimer;
        }

        private void PrayerTimerOptions_Load(object sender, EventArgs e)
        {
            if (this.Visible) this.Location = FormHelper.GetCenteredChildPositionRelativeToParentWorkAreaBound(this, formSettings);
        }

        private void checkBoxNotifySound_CheckedChanged(object sender, EventArgs e)
        {
            textBoxSoundName.Visible
                = buttonChangeSound.Visible
                = prayerTimer.Settings.Value.FavorSettings.FavorNotifySound
                = checkBoxNotifySound.Checked;
            prayerTimer.Settings.DelayedSave();
        }

        private void checkBoxNotifyPopup_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxPopupPersist.Visible
                = prayerTimer.Settings.Value.FavorSettings.FavorNotifyPopup
                = checkBoxNotifyPopup.Checked;
            prayerTimer.Settings.DelayedSave();
        }

        private void buttonChangeSound_Click(object sender, EventArgs e)
        {
            string soundName = SoundBank.ChooseSound();
            if (soundName != null)
            {
                textBoxSoundName.Text = soundName;
                prayerTimer.Settings.Value.FavorSettings.FavorNotifySoundName = soundName;
                prayerTimer.ForceUpdateFavorNotify(soundName:soundName);
                prayerTimer.Settings.DelayedSave();
            }
        }

        private void checkBoxPopupPersist_CheckedChanged(object sender, EventArgs e)
        {
            prayerTimer.Settings.Value.FavorSettings.FavorNotifyPopupPersist = checkBoxPopupPersist.Checked;
            prayerTimer.ForceUpdateFavorNotify(popupPersistent:checkBoxPopupPersist.Checked);
            prayerTimer.Settings.DelayedSave();
        }

        private void numericUpDownFavorWhenThis_ValueChanged(object sender, EventArgs e)
        {
            prayerTimer.Settings.Value.FavorSettings.FavorNotifyOnLevel
                = GeneralHelper.ConstrainValue((float)numericUpDownFavorWhenThis.Value, 0F, 100F);
            prayerTimer.Settings.DelayedSave();
        }

        private void checkBoxFavorWhenMAX_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownFavorWhenThis.Enabled = !checkBoxFavorWhenMAX.Checked;
            prayerTimer.Settings.Value.FavorSettings.FavorNotifyWhenMAX = checkBoxFavorWhenMAX.Checked;
            prayerTimer.Settings.DelayedSave();
        }

        private void checkBoxShowFaithSkill_CheckedChanged(object sender, EventArgs e)
        {
            prayerTimer.ShowFaithSkillOnTimer = checkBoxShowFaithSkill.Checked;
        }
    }
}
