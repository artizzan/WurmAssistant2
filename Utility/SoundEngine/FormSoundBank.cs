using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Aldurcraft.Utility.SoundEngine
{
    public partial class FormSoundBank : Form
    {
        List<string> AllSoundsList = new List<string>();

        public FormSoundBank()
        {
            InitializeComponent();
            SoundBank.BuildSoundBank();
            trackBarAdjustedVolume.Enabled = false;
            Init();
        }

        private void Init()
        {
            int savedSelectIndex = listBoxAllSounds.SelectedIndex;
            listBoxAllSounds.ClearSelected();
            AllSoundsList.Clear();
            listBoxAllSounds.Items.Clear();
            AllSoundsList.AddRange(SoundBank.GetSoundsArray());
            FillListAndAppendAdjVolumeValue();
        }

        private void FillListAndAppendAdjVolumeValue()
        {
            foreach (var snd in AllSoundsList)
            {
                listBoxAllSounds.Items.Add(snd + " (" + (int)(SoundBank.GetVolumeForSound(snd) * 100) + "%)");
            }
        }

        private void listBoxAllSounds_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxAllSounds.SelectedItems.Count > 0)
            {
                trackBarAdjustedVolume.Enabled = true;
                textBoxSelectedSound.Text = AllSoundsList[listBoxAllSounds.SelectedIndex];
                trackBarAdjustedVolume.Value = (int)(SoundBank.GetVolumeForSound(textBoxSelectedSound.Text) * 100);
            }
            else
            {
                textBoxSelectedSound.Text = "";
                trackBarAdjustedVolume.Value = 100;
                trackBarAdjustedVolume.Enabled = false;
            }
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            SoundBank.StopSounds();
            if (listBoxAllSounds.SelectedItems.Count > 0)
            {
                SoundBank.PlaySound(textBoxSelectedSound.Text);
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (listBoxAllSounds.SelectedItems.Count > 0)
            {
                SoundBank.RemoveSound(textBoxSelectedSound.Text);
                Init();
            }
        }

        private void buttonAddSounds_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var filename in openFileDialog1.FileNames)
                {
                    SoundBank.AddSound(filename);
                }
                SoundBank.BuildSoundBank();
                Init();
            }
        }

        private void listBoxAllSounds_KeyDown(object sender, KeyEventArgs e)
        {
            if (listBoxAllSounds.SelectedItems.Count > 0)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    SoundBank.RemoveSound(textBoxSelectedSound.Text);
                    Init();
                }
            }
        }

        private void trackBarAdjustedVolume_ValueChanged(object sender, EventArgs e)
        {
            
        }

        private void updateAdjustedVolumeForSelected()
        {
            string cursnd = AllSoundsList[listBoxAllSounds.SelectedIndex];
            cursnd += " (" + (int)(SoundBank.GetVolumeForSound(AllSoundsList[listBoxAllSounds.SelectedIndex]) * 100) + "%)";
            listBoxAllSounds.Items[listBoxAllSounds.SelectedIndex] = cursnd;
            listBoxAllSounds.Refresh();
        }

        private void buttonRename_Click(object sender, EventArgs e)
        {
            if (listBoxAllSounds.SelectedItems.Count > 0)
            {
                FormSoundBankRename RenameDialog = new FormSoundBankRename(textBoxSelectedSound.Text);
                if (RenameDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (RenameDialog.newpath != null && Path.GetFileName(RenameDialog.newpath) != textBoxSelectedSound.Text)
                    {
                        string oldpath = SoundBank.SoundsDirectory + @"\" + textBoxSelectedSound.Text;
                        string newpath = RenameDialog.newpath;
                        SoundBank.RenameSound(oldpath, newpath);
                        Init();
                    }
                }
            }
        }

        private void trackBarAdjustedVolume_MouseUp(object sender, MouseEventArgs e)
        {
            if (listBoxAllSounds.SelectedItems.Count > 0)
            {
                SoundBank.AdjustVolumeForSound(textBoxSelectedSound.Text, ((float)trackBarAdjustedVolume.Value / 100));
                updateAdjustedVolumeForSelected();
            }
        }

        private void trackBarAdjustedVolume_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) e.SuppressKeyPress = true;
            if (e.KeyCode == Keys.Right) e.SuppressKeyPress = true;
            if (e.KeyCode == Keys.Up) e.SuppressKeyPress = true;
            if (e.KeyCode == Keys.Down) e.SuppressKeyPress = true;
        }

        private void listBoxAllSounds_DoubleClick(object sender, EventArgs e)
        {
            buttonPlay.PerformClick();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SoundBank.StopSounds();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void FormSoundBank_Load(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(trackBarAdjustedVolume, "Adjust volume for this sound,\r\nthis is in addition to global volume.");
        }
    }
}
