using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Aldurcraft.WurmOnline.WurmAssistant2
{
    public partial class FormSettings : Form
    {
        bool formInited = false;
        public FormSettings()
        {
            InitializeComponent();
            checkBoxWebsiteOnMajorUpdate.Checked = AssistantEngine.Settings.Value.NotifyOnNewFeatures;
            checkBoxChangelogOnUpdate.Checked = AssistantEngine.Settings.Value.ChangelogOnEveryUpdate;
            checkBoxMinimizeToTray.Checked = AssistantEngine.Settings.Value.MiminizeToTray;
            checkBoxStartMinimized.Checked = AssistantEngine.Settings.Value.StartMinimized;
            checkBoxAlwaysShowTrayIcon.Checked = AssistantEngine.Settings.Value.AlwaysShowNotifyIcon;
            checkBoxConfirmAppExit.Checked = AssistantEngine.Settings.Value.PromptOnExit;
            checkBoxHideBeerButton.Checked = AssistantEngine.BeerButtonHidden;
            checkBoxDisableWebFeedScan.Checked = AssistantEngine.WebFeedDisabled;
            formInited = true;
        }

        private void checkBoxWebsiteOnMajorUpdate_CheckedChanged(object sender, EventArgs e)
        {
            AssistantEngine.Settings.Value.NotifyOnNewFeatures = checkBoxWebsiteOnMajorUpdate.Checked;
        }

        private void checkBoxChangelogOnUpdate_CheckedChanged(object sender, EventArgs e)
        {
            AssistantEngine.Settings.Value.ChangelogOnEveryUpdate = checkBoxChangelogOnUpdate.Checked;
        }

        private void checkBoxMinimizeToTray_CheckedChanged(object sender, EventArgs e)
        {
            AssistantEngine.Settings.Value.MiminizeToTray = checkBoxMinimizeToTray.Checked;
        }

        private void checkBoxStartMinimized_CheckedChanged(object sender, EventArgs e)
        {
            AssistantEngine.Settings.Value.StartMinimized = checkBoxStartMinimized.Checked;
        }

        private void checkBoxAlwaysShowTrayIcon_CheckedChanged(object sender, EventArgs e)
        {
            AssistantEngine.Settings.Value.AlwaysShowNotifyIcon = checkBoxAlwaysShowTrayIcon.Checked;
        }

        private void checkBoxConfirmAppExit_CheckedChanged(object sender, EventArgs e)
        {
            AssistantEngine.Settings.Value.PromptOnExit = checkBoxConfirmAppExit.Checked;
        }

        private void checkBoxHideBeerButton_CheckedChanged(object sender, EventArgs e)
        {
            AssistantEngine.BeerButtonHidden = checkBoxHideBeerButton.Checked;
        }

        private void checkBoxDisableWebFeedScan_CheckedChanged(object sender, EventArgs e)
        {
            AssistantEngine.WebFeedDisabled = checkBoxDisableWebFeedScan.Checked;
        }

        private void FormSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (formInited) AssistantEngine.Settings.DelayedSave();
        }
    }
}
