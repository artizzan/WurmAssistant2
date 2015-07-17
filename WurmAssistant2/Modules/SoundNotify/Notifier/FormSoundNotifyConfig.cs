using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using Aldurcraft.Utility.SoundEngine;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.SoundNotify
{
    public partial class FormSoundNotifyConfig : Form
    {
        SoundNotifier ParentModule;

        public FormSoundNotifyConfig()
        {
            InitializeComponent();
        }

        public FormSoundNotifyConfig(SoundNotifier module)
            : this()
        {
            this.ParentModule = module;
            this.Text = String.Format("Sound Notify ({0})", ParentModule.Player);
            numericUpDownQueueDelay.Value = Convert.ToDecimal(ParentModule.Settings.Value.QueueDefDelay);
            textBoxQueSoundName.Text = ParentModule.GetQueueSoundForUI();
            UpdateMutedState();
        }

        private void RefreshBankAndList()
        {
            SoundBank.RebuildSoundBank();
            RefreshList();
        }

        private void RefreshList()
        {
            listViewSounds.Items.Clear();
            List<PlaylistEntry> playlist = ParentModule.getPlaylist();
            int counter = 1;
            foreach (PlaylistEntry entry in playlist)
            {
                listViewSounds.Items.Add(counter.ToString());
                counter++;
                listViewSounds.Items[listViewSounds.Items.Count - 1].SubItems.Add(entry.SoundName);

                if (entry.isCustomRegex) listViewSounds.Items[listViewSounds.Items.Count - 1].SubItems.Add(entry.Condition);
                else listViewSounds.Items[listViewSounds.Items.Count - 1].SubItems.Add(ParentModule.ConvertRegexToCondOutput(entry.Condition));

                string allspecials = "";
                bool firstspecial = true;
                foreach (string special in entry.SpecialSettings)
                {
                    if (firstspecial)
                    {
                        allspecials = special;
                        firstspecial = false;
                    }
                    else allspecials += ", " + special;
                }
                listViewSounds.Items[listViewSounds.Items.Count - 1].SubItems.Add(allspecials);
                listViewSounds.Items[listViewSounds.Items.Count - 1].SubItems.Add(entry.isActive.ToString());
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            RefreshBankAndList();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            FormSoundNotifyConfigDialog dialog = new FormSoundNotifyConfigDialog(ParentModule, FormSoundNotifyConfigDialogMode.Add);
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string condition = dialog.textBoxChooseCond.Text;
                List<string> specCond = new List<string>();

                if (dialog.checkBoxUseRegexSemantics.Checked)
                {
                    specCond.Add("s:CustomRegex");
                }
                else condition = ParentModule.ConvertCondOutputToRegex(condition);

                if (dialog.checkedListBoxSearchIn.CheckedItems.Count > 0)
                {
                    foreach (string line in dialog.checkedListBoxSearchIn.CheckedItems)
                    {
                        specCond.Add(line);
                    }
                }
                ParentModule.AddPlaylistEntry(dialog.listBoxChooseSound.Text, condition, specCond, true);
                RefreshBankAndList();
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (listViewSounds.SelectedItems.Count > 0)
            {
                int oldEntryIndex = Convert.ToInt32(listViewSounds.SelectedItems[0].Text) - 1;
                bool oldActive = ParentModule.getPlaylistEntryAtIndex(oldEntryIndex).isActive;
                FormSoundNotifyConfigDialog dialog = new FormSoundNotifyConfigDialog(
                    ParentModule,
                    FormSoundNotifyConfigDialogMode.Edit,
                    ParentModule.getPlaylistEntryAtIndex(oldEntryIndex));
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string condition = dialog.textBoxChooseCond.Text;
                    ParentModule.RemovePlaylistEntry(oldEntryIndex);
                    List<string> specCond = new List<string>();

                    if (dialog.checkBoxUseRegexSemantics.Checked)
                    {
                        specCond.Add("s:CustomRegex");
                    }
                    else condition = ParentModule.ConvertCondOutputToRegex(condition);

                    if (dialog.checkedListBoxSearchIn.CheckedItems.Count > 0)
                    {
                        foreach (string line in dialog.checkedListBoxSearchIn.CheckedItems)
                        {
                            specCond.Add(line);
                        }
                    }
                    ParentModule.AddPlaylistEntry(dialog.listBoxChooseSound.Text, condition, specCond, oldActive, oldEntryIndex);
                    RefreshBankAndList();
                }
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            removeEntry(false);
        }

        private void FormSoundNotifyConfig_Load(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(checkBoxToggleQSound, "This option will play a sound, when you finish your crafting queue in the game");
            toolTip1.SetToolTip(numericUpDownQueueDelay, "Once your crafting queue finishes in game, \r\nthis amount of seconds will Sound Notify wait, \r\nbefore playing the sound");
            RefreshBankAndList();
            RefreshQueueSoundUI();
            if (OperatingSystemInfo.RunningOS == OperatingSystemInfo.OStype.WinXP)
            {
                this.Size = new Size(this.Size.Width + 190, this.Size.Height);
            }
        }

        private void RefreshQueueSoundUI()
        {
            if (ParentModule.Settings.Value.QueueSoundEnabled)
            {
                checkBoxToggleQSound.Checked = true;
            }
            else
            {
                checkBoxToggleQSound.Checked = false;
            }
        }

        private void numericUpDownQueueDelay_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDownQueueDelay.Value < 0) numericUpDownQueueDelay.Value = 0;
            ParentModule.Settings.Value.QueueDefDelay = Convert.ToDouble(numericUpDownQueueDelay.Value);
            ParentModule.Settings.DelayedSave();
        }

        private void checkBoxToggleQSound_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxToggleQSound.Checked)
            {
                ParentModule.Settings.Value.QueueSoundEnabled = true;
            }
            else ParentModule.Settings.Value.QueueSoundEnabled = false;

            ParentModule.Settings.DelayedSave();
            RefreshQueueSoundUI();
        }

        void removeEntry(bool ifShowWarning)
        {
            if (listViewSounds.SelectedItems.Count > 0)
            {
                if (ifShowWarning && MessageBox.Show("Delete this sound? \n\nNote: You can use DEL key to avoid this popup", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                {
                    ParentModule.RemovePlaylistEntry(Convert.ToInt32(listViewSounds.SelectedItems[0].Text) - 1);
                    RefreshBankAndList();
                }
                else if (!ifShowWarning)
                {
                    ParentModule.RemovePlaylistEntry(Convert.ToInt32(listViewSounds.SelectedItems[0].Text) - 1);
                    RefreshBankAndList();
                }
            }
        }

        private void listViewSounds_DoubleClick(object sender, EventArgs e)
        {
            buttonEdit.PerformClick();
        }

        private void listViewSounds_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                removeEntry(false);
        }

        public void RestoreFromMin()
        {
            if (this.WindowState == FormWindowState.Minimized) this.WindowState = FormWindowState.Normal;
        }

        private void buttonMute_Click(object sender, EventArgs e)
        {
            ParentModule.Settings.Value.Muted = !ParentModule.Settings.Value.Muted;
            ParentModule.Settings.DelayedSave();
            UpdateMutedState();
        }

        public void UpdateMutedState()
        {
            if (!ParentModule.Settings.Value.Muted)
            {
                buttonMute.Image = Properties.Resources.SoundEnabledSmall;
                this.Text = String.Format("Sound Notify ({0})", ParentModule.Player);
            }
            else
            {
                SoundBank.StopSounds();
                buttonMute.Image = Properties.Resources.SoundDisabledSmall;
                this.Text = String.Format("Sound Notify ({0}) [MUTED]", ParentModule.Player);
            }
            ParentModule.UpdateMutedState();
        }

        private void buttonManageSNDBank_Click(object sender, EventArgs e)
        {
            SoundBank.OpenSoundBank();
        }

        private void listViewSounds_MouseClick(object sender, MouseEventArgs e)
        {
            // swap active/inactive on the sound
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ParentModule.TogglePlaylistEntryActive(Convert.ToInt32(listViewSounds.SelectedItems[0].Text) - 1);
                RefreshList();
            }
        }

        private void buttonChangeQueSound_Click(object sender, EventArgs e)
        {
            ParentModule.SetQueueSound();
        }

        public void UpdateSoundName(string name)
        {
            this.textBoxQueSoundName.Text = name;
        }

        private void buttonQueSoundHelp_Click(object sender, EventArgs e)
        {
        }

        private void buttonPlaylistHelp_Click(object sender, EventArgs e)
        {
        }

        private void buttonClearQueSound_Click(object sender, EventArgs e)
        {
            ParentModule.Settings.Value.QueueSoundName = null;
            ParentModule.Settings.DelayedSave();
            textBoxQueSoundName.Text = "default";
        }

        private void FormSoundNotifyConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing) 
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void buttonAdvancedOptions_Click(object sender, EventArgs e)
        {
            panel1.Visible = !panel1.Visible;
        }

        private void buttonModifyQueueTriggers_Click(object sender, EventArgs e)
        {
            LogQueueParseHelper.EditModFile();
        }
    }
}
