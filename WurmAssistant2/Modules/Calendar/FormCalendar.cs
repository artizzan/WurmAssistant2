using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using Aldurcraft.WurmOnline.WurmState;
using Aldurcraft.Utility.SoundEngine;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Calendar
{
    public partial class FormCalendar : Form
    {
        ModuleCalendar ParentModule;

        bool _WindowInitCompleted = false;
        bool serverListCreated = false;
        public FormCalendar(ModuleCalendar parentModule)
        {
            InitializeComponent();
            this.ParentModule = parentModule;
            this.Size = ParentModule.Settings.Value.MainWindowSize;
            radioButtonWurmTime.Checked = ParentModule.Settings.Value.UseWurmTimeForDisplay;
            radioButtonRealTime.Checked = !ParentModule.Settings.Value.UseWurmTimeForDisplay;
            checkBoxSoundWarning.Checked = ParentModule.Settings.Value.SoundWarning;
            checkBoxPopupWarning.Checked = ParentModule.Settings.Value.PopupWarning;
            textBoxChosenSound.Text = ParentModule.Settings.Value.SoundName;

            CreateServerListAsync();
            _WindowInitCompleted = true;
        }

        private async Task CreateServerListAsync()
        {
            try
            {
                string[] allServers = await Aldurcraft.WurmOnline.WurmState.WurmServer.GetAllServerNamesAsync();
                comboBoxChooseServer.Items.AddRange(allServers);
                serverListCreated = true;
                comboBoxChooseServer.Enabled = true;
                comboBoxChooseServer.Text = ParentModule.Settings.Value.ServerName;
            }
            catch (Exception _e)
            {
                Logger.LogError("CreateServerList problem", this, _e);
            }
        }

        private void comboBoxChooseServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            ParentModule.Settings.Value.ServerName = comboBoxChooseServer.Text;
            ParentModule.InitCachedWDT();
            ParentModule.Settings.DelayedSave();
        }

        public void RestoreFromMin()
        {
            if (this.WindowState == FormWindowState.Minimized) this.WindowState = FormWindowState.Normal;
        }

        public void UpdateSeasonOutput(List<ModuleCalendar.WurmSeasonOutputItem> outputList, bool wurmTime)
        {
            //ListView + double buffering
            //http://stackoverflow.com/questions/442817/c-sharp-flickering-listview-on-update
            ////
            listViewNFSeasons.Items.Clear();
            foreach (var item in outputList)
            {
                listViewNFSeasons.Items.Add(new ListViewItem(new string[] {
                    item.BuildName(), item.BuildTimeData(wurmTime), item.BuildLengthData(wurmTime) }));
            }
            //wurm date debug
            try
            {
                if (serverListCreated) textBoxWurmDate.Text = ParentModule.cachedWDT.ToString();
            }
            catch
            {
                textBoxWurmDate.Text = "error";
            }
        }

        private void radioButtonWurmTime_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonWurmTime.Checked)
            {
                ParentModule.Settings.Value.UseWurmTimeForDisplay = true;
                labelDisplayTimeMode.Text = "Showing times as wurm time";
                ParentModule.Settings.DelayedSave();
            }
        }

        private void radioButtonRealTime_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonRealTime.Checked)
            {
                ParentModule.Settings.Value.UseWurmTimeForDisplay = false;
                labelDisplayTimeMode.Text = "Showing times as real time";
                ParentModule.Settings.DelayedSave();
            }
        }

        private void buttonChooseSeasons_Click(object sender, EventArgs e)
        {
            ParentModule.ChooseTrackedSeasons();
        }

        public void UpdateTrackedSeasonsList(string[] trackedSeasons)
        {
            if (trackedSeasons.Length > 0)
            {
                StringBuilder builder = new StringBuilder(120);
                foreach (string str in trackedSeasons)
                {
                    builder.Append(str).Append(", ");
                }
                builder.Remove(builder.Length - 2, 2);
                textBoxChosenSeasons.Text = builder.ToString();
            }
            else textBoxChosenSeasons.Text = "none";
        }

        private void buttonChooseSound_Click(object sender, EventArgs e)
        {
            FormChooseSound ChooseSoundUI = new FormChooseSound();
            if (ChooseSoundUI.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ParentModule.Settings.Value.SoundName = ChooseSoundUI.ChosenSound;
                textBoxChosenSound.Text = ParentModule.Settings.Value.SoundName;
                ParentModule.Settings.DelayedSave();
            }
        }

        private void buttonClearSound_Click(object sender, EventArgs e)
        {
            ParentModule.Settings.Value.SoundName = "none";
            ParentModule.Settings.DelayedSave();
            textBoxChosenSound.Text = ParentModule.Settings.Value.SoundName;
        }

        private void checkBoxSoundWarning_CheckedChanged(object sender, EventArgs e)
        {
            ParentModule.Settings.Value.SoundWarning = checkBoxSoundWarning.Checked;
            ParentModule.Settings.DelayedSave();
        }

        private void checkBoxPopupWarning_CheckedChanged(object sender, EventArgs e)
        {
            ParentModule.Settings.Value.PopupWarning = checkBoxPopupWarning.Checked;
            ParentModule.Settings.DelayedSave();
        }

        private void FormCalendar_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void buttonConfigure_Click(object sender, EventArgs e)
        {
            panelOptions.Visible = !panelOptions.Visible;
        }

        private void FormCalendar_Load(object sender, EventArgs e)
        {
            if (panelOptions.Visible) panelOptions.Visible = false;
            if (OperatingSystemInfo.RunningOS == OperatingSystemInfo.OStype.WinXP)
            {
                Logger.LogInfo("adjusting layout for WinXP", this);
                this.Size = new Size(this.Size.Width + 110, this.Size.Height);
            }
        }

        private void buttonModSeasonList_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show("You will need to restart this Module or restart Assistant for these changed to take effect\r\n"
                    + "Mod file is located at this path:\r\n" + ParentModule.ModFilePath + "\r\n");
                System.Diagnostics.Process.Start(ParentModule.ModFilePath);
            }
            catch (Exception _e)
            {
                Logger.LogError("problem opening mod-file", this, _e);
            }
        }

        private void FormCalendar_Resize(object sender, EventArgs e)
        {
            try
            {
                if (_WindowInitCompleted)
                {
                    ParentModule.Settings.Value.MainWindowSize = this.Size;
                    ParentModule.Settings.DelayedSave();
                }
            }
            catch (Exception _e)
            {
                Logger.LogError("FormCalendar_Resize", this, _e);
                throw;
            }
        }
    }
}
