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
using Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    public partial class FormTriggersMain : Form
    {
        ModuleTriggers ParentModule;
        public FormTriggersMain(ModuleTriggers parentModule)
        {
            this.ParentModule = parentModule;
            InitializeComponent();
            trackBarGlobalVolume.Value = 
                GeneralHelper.ConstrainValue<int>((int)(ParentModule.Settings.Value.GlobalVolume * 100), 0, 100);
            SoundBank.GlobalVolume = ParentModule.Settings.Value.GlobalVolume;
            UpdateMuteIcon();
        }

        private void FormSoundNotifyMain_Load(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(buttonMute, "Mute/unmute all sounds");
            toolTip1.SetToolTip(buttonAddNew, "Add new sound managers for more wurm characters");
        }

        public void AddNotifierController(UcPlayerTriggersController controller)
        {
            flowLayoutPanel1.Controls.Add(controller);
        }

        public void RemoveNotifierController(UcPlayerTriggersController controller)
        {
            flowLayoutPanel1.Controls.Remove(controller);
        }

        private void buttonAddNew_Click(object sender, EventArgs e)
        {
            //show dialog to choose player
            ParentModule.AddNewNotifier();
        }

        public void RestoreFromMin()
        {
            if (this.WindowState == FormWindowState.Minimized) this.WindowState = FormWindowState.Normal;
        }

        private void FormSoundNotifyMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void buttonMute_Click(object sender, EventArgs e)
        {
            ParentModule.Settings.Value.GlobalMute = !ParentModule.Settings.Value.GlobalMute;
            ParentModule.Settings.DelayedSave();
            UpdateMuteIcon();
        }

        void UpdateMuteIcon()
        {
            if (ParentModule.Settings.Value.GlobalMute) 
                this.buttonMute.BackgroundImage = Properties.Resources.SoundDisabledSmall;
            else this.buttonMute.BackgroundImage = Properties.Resources.SoundEnabledSmall;
        }

        private void trackBarGlobalVolume_Scroll(object sender, EventArgs e)
        {
            ParentModule.Settings.Value.GlobalVolume = GeneralHelper.ConstrainValue<float>(
                (float)trackBarGlobalVolume.Value / 100, 0F, 1.0F);
            ParentModule.Settings.DelayedSave();
        }

        private void buttonManageSounds_Click(object sender, EventArgs e)
        {
            SoundBank.OpenSoundBank();
        }
    }
}
