using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Aldurcraft.WurmOnline.WurmState;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2
{
    public partial class FormConfigWizard : Form
    {
        public class Results
        {
            internal WurmClient.Configs.EnumLoggingType? LoggingType = null;
            internal bool? TimestampMessages = null;
            internal bool? FavorAndAlignmentUpdates = null;
            internal WurmClient.Configs.EnumSkillGainRate? SkillGainRate = null;
            internal bool? OnUpdateShowNewFeatures = null;
            internal bool? OnUpdateShowFullChangelog = null;
            public string OverrideWurmDir = null;
        }

        enum ConfigMode_Modes { NoneSet, Auto, Manual }

        //used to apply modifications to wurm log file and then to return other settings back to engine
        public Results results = new Results();

        bool wizardCompleted = false;

        //config is either auto, where recommended values are set or manual where any setting can be reviewed by user
        ConfigMode_Modes configMode_mode = ConfigMode_Modes.NoneSet;

        Form _parent;

        public FormConfigWizard(Form parent = null)
        {
            _parent = parent;
            InitializeComponent();

            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;

            WurmPath_textBoxPath.Text = WurmClient.WurmPaths.WurmDir;
            WurmPath_checkIfEnableButtonNext();
        }

        #region WURM PATH

        // wurm dir must be correct before this part can be passed

        void WurmPath_checkIfEnableButtonNext()
        {
            if (WurmClient.InitSuccessful) WurmPath_buttonNext.Enabled = true;
            else WurmPath_buttonNext.Enabled = false;
        }

        private void WurmPath_buttonOpenInExplorer_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(WurmClient.WurmPaths.WurmDir);
            }
            catch (Exception _e)
            {
                Logger.LogError("failed opening wurmDir in wizard", this, _e);
                MessageBox.Show("Could not open this directory: " + _e.Message);
            }
        }

        private void WurmPath_buttonChangePath_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialogWurmPath.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                WurmPath_textBoxPath.Text = folderBrowserDialogWurmPath.SelectedPath;
                WurmPath_buttonReset.Visible = true;
                bool initsuccess = WurmClient.OverrideWurmDir(WurmPath_textBoxPath.Text);
                WurmPath_checkIfEnableButtonNext();
                if (!initsuccess)
                {
                    MessageBox.Show("This does not appear to be correct wurm dir, please double-check and if confirmed, report this error", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    results.OverrideWurmDir = WurmPath_textBoxPath.Text;
                }
            }
        }

        private void WurmPath_buttonExit_Click(object sender, EventArgs e)
        {
            results = null;
            this.Close();
        }

        private void WurmPath_buttonReset_Click(object sender, EventArgs e)
        {
            WurmPath_buttonReset.Visible = false;
            WurmClient.OverrideWurmDir(null);
            WurmPath_textBoxPath.Text = WurmClient.WurmPaths.WurmDir;
            WurmPath_checkIfEnableButtonNext();
        }

        private void WurmPath_buttonNext_Click(object sender, EventArgs e)
        {
            tabControlWizard.SelectTab(tabPageConfigMode);
        }

        #endregion

        #region CONFIG MODE

        private void ConfigMode_Back_Click(object sender, EventArgs e)
        {
            tabControlWizard.SelectTab(tabPageWurmPath);
        }

        private void ConfigMode_buttonNext_Click(object sender, EventArgs e)
        {
            tabControlWizard.SelectTab(tabPageCloseClients);
            CloseClients_is_CurrentTab = true;
        }

        private void ConfigMode_radioButtonAuto_CheckedChanged(object sender, EventArgs e)
        {
            ConfigMode_CheckIfEnableButtonNext();
            ConfigMode_SetMode();
        }

        private void ConfigMode_radioButtonManual_CheckedChanged(object sender, EventArgs e)
        {
            ConfigMode_CheckIfEnableButtonNext();
            ConfigMode_SetMode();
        }

        void ConfigMode_CheckIfEnableButtonNext()
        {
            if (ConfigMode_radioButtonAuto.Checked || ConfigMode_radioButtonManual.Checked)
                ConfigMode_buttonNext.Enabled = true;
            else ConfigMode_buttonNext.Enabled = false;
        }

        void ConfigMode_SetMode()
        {
            if (ConfigMode_radioButtonAuto.Checked)
            {
                configMode_mode = ConfigMode_Modes.Auto;
                CloseClients_timer.Enabled = true;
            }
            else if (ConfigMode_radioButtonManual.Checked)
            {
                configMode_mode = ConfigMode_Modes.Manual;
                CloseClients_timer.Enabled = true;
            }
            else
            {
                configMode_mode = ConfigMode_Modes.NoneSet;
            }
        }

        #endregion

        #region CLOSE OPEN WURM CLIENTS

        //wizard cannot advance until all cliens are closed

        bool CloseClients_is_CurrentTab = false;

        private void CloseClients_buttonExit_Click(object sender, EventArgs e)
        {
            CloseClients_is_CurrentTab = false;
            tabControlWizard.SelectTab(tabPageConfigMode);
        }

        private void CloseClients_buttonNext_Click(object sender, EventArgs e)
        {
            CloseClients_is_CurrentTab = false;
            CloseClients_timer.Enabled = false;
            if (configMode_mode == ConfigMode_Modes.Manual)
            {
                tabControlWizard.SelectTab(tabPageLoggingMode);
                LoggingMode_AutoadvanceIfSetCorrectly();
            }
            else if (configMode_mode == ConfigMode_Modes.Auto)
            {
                ApplyConfig();
            }
        }

        private void CloseClients_timer_Tick(object sender, EventArgs e)
        {
            if (CloseClients_is_CurrentTab)
            {
                if (WurmClient.State.WurmClientRunning == WurmClient.State.EnumWurmClientStatus.NotRunning)
                {
                    CloseClients_buttonNext.Enabled = true;
                    CloseClients_textBoxClientsRunning.Text = "No Wurm Clients running, please continue.";
                    CloseClients_buttonNext.PerformClick();
                }
                else if (WurmClient.State.WurmClientRunning == WurmClient.State.EnumWurmClientStatus.Running)
                {
                    CloseClients_buttonNext.Enabled = false;
                    CloseClients_textBoxClientsRunning.Text = "There are still Wurm Online Clients running, please close them.";
                }
                else
                {
                    CloseClients_buttonNext.Enabled = false;
                    CloseClients_textBoxClientsRunning.Text = "PROBLEM: Could not check if wurm clients are running!\r\n\r\nPlease report this issue!";
                }
            }
        }

        #endregion

        #region LOGGING MODE

        private void LoggingMode_AutoadvanceIfSetCorrectly()
        {
            WurmClient.Configs.ConfigData[] allconfigs = WurmClient.Configs.GetAllConfigs();

            WurmClient.Configs.EnumLoggingType[] allowedValues = 
            { 
                WurmClient.Configs.EnumLoggingType.Daily, 
                WurmClient.Configs.EnumLoggingType.Monthly 
            };

            bool configCorrect = true;
            foreach (var config in allconfigs)
            {
                if (!config.EventAndOtherLoggingModesAreEqual(allowedValues))
                {
                    configCorrect = false; break;
                }
            }

            if (configCorrect)
            {
                LoggingMode_radioButtonKeepOld.Checked = true;
                LoggingMode_buttonNext.PerformClick();
            }
        }

        private void LoggingMode_buttonExit_Click(object sender, EventArgs e)
        {
            results = null;
            this.Close();
        }

        private void LoggingMode_radioButtonMonthly_CheckedChanged(object sender, EventArgs e)
        {
            LoggingMode_UpdateResults();
        }

        private void LoggingMode_radioButtonDaily_CheckedChanged(object sender, EventArgs e)
        {
            LoggingMode_UpdateResults();
        }

        private void LoggingMode_radioButtonKeepOld_CheckedChanged(object sender, EventArgs e)
        {
            LoggingMode_UpdateResults();
        }

        private void LoggingMode_UpdateResults()
        {
            LoggingMode_buttonNext.Enabled = true;
            if (LoggingMode_radioButtonMonthly.Checked) results.LoggingType = WurmClient.Configs.EnumLoggingType.Monthly;
            else if (LoggingMode_radioButtonDaily.Checked) results.LoggingType = WurmClient.Configs.EnumLoggingType.Daily;
            else results.LoggingType = null;
        }

        private void LoggingMode_buttonNext_Click(object sender, EventArgs e)
        {
            tabControlWizard.SelectTab(tabPageTimestamps);
            Timestamps_AutoadvanceIfSetCorrectly();
        }

        #endregion

        #region TIMESTAMP MESSAGES

        private void Timestamps_AutoadvanceIfSetCorrectly()
        {
            WurmClient.Configs.ConfigData[] allconfigs = WurmClient.Configs.GetAllConfigs();

            bool configCorrect = true;
            foreach (var config in allconfigs)
            {
                if (config.TimestampMessages == null || config.TimestampMessages.Value == false)
                {
                    configCorrect = false; break;
                }
            }

            if (configCorrect)
            {
                Timestamps_radioButtonKeepUnchanged.Checked = true;
                Timestamps_buttonNext.PerformClick();
            }
        }

        private void Timestamps_buttonBack_Click(object sender, EventArgs e)
        {
            tabControlWizard.SelectTab(tabPageLoggingMode);
        }

        private void Timestamps_radioButtonEnabled_CheckedChanged(object sender, EventArgs e)
        {
            Timestamps_UpdateResults();
        }

        private void Timestamps_radioButtonDisabled_CheckedChanged(object sender, EventArgs e)
        {
            Timestamps_UpdateResults();
        }

        private void Timestamps_radioButtonKeepUnchanged_CheckedChanged(object sender, EventArgs e)
        {
            Timestamps_UpdateResults();
        }

        private void Timestamps_UpdateResults()
        {
            Timestamps_buttonNext.Enabled = true;
            if (Timestamps_radioButtonEnabled.Checked) results.TimestampMessages = true;
            else if (Timestamps_radioButtonDisabled.Checked) results.TimestampMessages = false;
            else results.TimestampMessages = null;
        }

        private void Timestamps_buttonNext_Click(object sender, EventArgs e)
        {
            tabControlWizard.SelectTab(tabPageFavorAlignUpdates);
            FavorAlignUpdates_AutoadvanceIfSetCorrectly();
        }

        #endregion

        #region FAVOR AND ALIGNMENT UPDATES

        private void FavorAlignUpdates_AutoadvanceIfSetCorrectly()
        {
            WurmClient.Configs.ConfigData[] allconfigs = WurmClient.Configs.GetAllConfigs();

            bool configCorrect = true;
            foreach (var config in allconfigs)
            {
                if (config.NoSkillMessageOnFavorChange == null || config.NoSkillMessageOnAlignmentChange == null
                    || config.NoSkillMessageOnFavorChange.Value == true
                    || config.NoSkillMessageOnAlignmentChange.Value == true)
                {
                    configCorrect = false; break;
                }
            }

            if (configCorrect)
            {
                FavorAlignUpdates_radioButtonKeepUnchanged.Checked = true;
                FavorAlignUpdates_buttonNext.PerformClick();
            }
        }

        private void FavorAlignUpdates_buttonBack_Click(object sender, EventArgs e)
        {
            tabControlWizard.SelectTab(tabPageTimestamps);
        }

        private void FavorAlignUpdates_radioButtonDontHide_CheckedChanged(object sender, EventArgs e)
        {
            FavorAlignUpdates_UpdateResults();
        }

        private void FavorAlignUpdates_radioButtonHide_CheckedChanged(object sender, EventArgs e)
        {
            FavorAlignUpdates_UpdateResults();
        }

        private void FavorAlignUpdates_radioButtonKeepUnchanged_CheckedChanged(object sender, EventArgs e)
        {
            FavorAlignUpdates_UpdateResults();
        }

        private void FavorAlignUpdates_UpdateResults()
        {
            FavorAlignUpdates_buttonNext.Enabled = true;
            if (FavorAlignUpdates_radioButtonDontHide.Checked) results.FavorAndAlignmentUpdates = true;
            else if (FavorAlignUpdates_radioButtonHide.Checked) results.FavorAndAlignmentUpdates = false;
            else results.FavorAndAlignmentUpdates = null;
        }

        private void FavorAlignUpdates_buttonNext_Click(object sender, EventArgs e)
        {
            tabControlWizard.SelectTab(tabPageSkillgainUpdates);
            SkillgainUpdates_AutoadvanceIfSetCorrectly();
        }

        #endregion

        #region SKILLGAIN UPDATES

        private void SkillgainUpdates_AutoadvanceIfSetCorrectly()
        {
            WurmClient.Configs.ConfigData[] allconfigs = WurmClient.Configs.GetAllConfigs();

            bool configCorrect = true;
            foreach (var config in allconfigs)
            {
                if (config.SkillGainRate != WurmClient.Configs.EnumSkillGainRate.Always)
                {
                    configCorrect = false; break;
                }
            }

            if (configCorrect)
            {
                SkillgainUpdates_radioButtonAlways.Checked = true;
                SkillgainUpdates_buttonNext.PerformClick();
            }
        }

        private void SkillgainUpdates_buttonBack_Click(object sender, EventArgs e)
        {
            tabControlWizard.SelectTab(tabPageFavorAlignUpdates);
        }

        private void SkillgainUpdates_radioButtonAlways_CheckedChanged(object sender, EventArgs e)
        {
            SkillgainUpdates_UpdateResults();
        }

        private void SkillgainUpdates_radioButtonPer0_001_CheckedChanged(object sender, EventArgs e)
        {
            SkillgainUpdates_UpdateResults();
        }

        private void SkillgainUpdates_radioButtonPer0_01_CheckedChanged(object sender, EventArgs e)
        {
            SkillgainUpdates_UpdateResults();
        }

        private void SkillgainUpdates_radioButtonPer0_1_CheckedChanged(object sender, EventArgs e)
        {
            SkillgainUpdates_UpdateResults();
        }

        private void SkillgainUpdates_radioButtonPer1_CheckedChanged(object sender, EventArgs e)
        {
            SkillgainUpdates_UpdateResults();
        }

        private void SkillgainUpdates_radioButtonNever_CheckedChanged(object sender, EventArgs e)
        {
            SkillgainUpdates_UpdateResults();
        }

        private void SkillgainUpdates_UpdateResults()
        {
            SkillgainUpdates_buttonNext.Enabled = true;
            if (SkillgainUpdates_radioButtonAlways.Checked)
                results.SkillGainRate = WurmClient.Configs.EnumSkillGainRate.Always;
            else if (SkillgainUpdates_radioButtonPer0_001.Checked)
                results.SkillGainRate = WurmClient.Configs.EnumSkillGainRate.per0_001;
            else if (SkillgainUpdates_radioButtonPer0_01.Checked)
                results.SkillGainRate = WurmClient.Configs.EnumSkillGainRate.Per0_01;
            else if (SkillgainUpdates_radioButtonPer0_1.Checked)
                results.SkillGainRate = WurmClient.Configs.EnumSkillGainRate.Per0_1;
            else if (SkillgainUpdates_radioButtonPer1.Checked)
                results.SkillGainRate = WurmClient.Configs.EnumSkillGainRate.PerInteger;
            else if (SkillgainUpdates_radioButtonNever.Checked)
                results.SkillGainRate = WurmClient.Configs.EnumSkillGainRate.Never;
            else results.SkillGainRate = null;
        }

        private void SkillgainUpdates_buttonNext_Click(object sender, EventArgs e)
        {
            tabControlWizard.SelectTab(tabPageAssistantUpdates);
            AssistantUpdates_UpdateResults();
        }

        #endregion

        #region ASSISTANT UPDATES

        private void AssistantUpdates_buttonBack_Click(object sender, EventArgs e)
        {
            tabControlWizard.SelectTab(tabPageSkillgainUpdates);
        }

        private void AssistantUpdates_checkBoxSummary_CheckedChanged(object sender, EventArgs e)
        {
            AssistantUpdates_UpdateResults();
        }

        private void AssistantUpdates_checkBoxChangelog_CheckedChanged(object sender, EventArgs e)
        {
            AssistantUpdates_UpdateResults();
        }

        private void AssistantUpdates_UpdateResults()
        {
            if (AssistantUpdates_checkBoxSummary.Checked) results.OnUpdateShowNewFeatures = true;
            else results.OnUpdateShowNewFeatures = false;

            if (AssistantUpdates_checkBoxChangelog.Checked) results.OnUpdateShowFullChangelog = true;
            else results.OnUpdateShowFullChangelog = false;
        }

        private void AssistantUpdates_buttonNext_Click(object sender, EventArgs e)
        {
            ApplyConfig();
        }

        #endregion

        #region APPLY AND FINISH

        private void ApplyConfig()
        {
            var allChanges = new List<string> {"Summary:"};
            //backup configs
            string[] configdirs = WurmClient.WurmPaths.GetConfigDirs();
            if (configdirs != null)
            {
                foreach (var dir in configdirs)
                {
                    if (File.Exists(Path.Combine(dir, "gamesettings.txt")))
                    {
                        try
                        {
                            File.Copy(Path.Combine(dir, "gamesettings.txt"), Path.Combine(dir, "gamesettings.txt.bak"), true);
                        }
                        catch (Exception exception)
                        {
                            Logger.LogError("problem creating backup of gamesettings.txt in " + dir, this, exception);
                        }
                    }
                }
            }
            else
            {
                Logger.LogCritical("no configs were available for wizard to modify", this);
                throw new Exception("no configs were available for wizard to modify");
            }

            if (configMode_mode == ConfigMode_Modes.Auto)
            {
                WurmClient.Configs.ConfigData[] allconfigs = WurmClient.Configs.GetAllConfigs();

                var allowedLogMode = new[]
                {
                    WurmClient.Configs.EnumLoggingType.Daily,
                    WurmClient.Configs.EnumLoggingType.Monthly
                };

                foreach (var config in allconfigs)
                {
                    allChanges.Add("Reviewing Wurm Client config: " + config.ConfigName);

                    if (!config.EventAndOtherLoggingModesAreEqual(allowedLogMode))
                    {
                        allChanges.Add(config.SetCommonLoggingMode(WurmClient.Configs.EnumLoggingType.Monthly)
                            ? "Wurm Client> Event and Other message logging to: Monthly files"
                            : "Wurm Client> ERROR: Failed to set message logging");
                    }
                    if (config.TimestampMessages != true)
                    {
                        allChanges.Add(config.SetTimestampMessages(true)
                            ? "Wurm Client> Timestamp Messages set to: true"
                            : "Wurm Client> ERROR: Failed to set timestamp messages");
                    }
                    if (config.NoSkillMessageOnFavorChange != false)
                    {
                        allChanges.Add(config.SetNoSkillMessageOnFavorChange(false)
                            ? "Wurm Client> Hide favor updates set to : false"
                            : "Wurm Client> ERROR: Failed to set favor updates");
                    }
                    if (config.NoSkillMessageOnAlignmentChange != false)
                    {
                        allChanges.Add(config.SetNoSkillMessageOnAlignmentChange(false)
                            ? "Wurm Client> Hide alignment updates set to : false"
                            : "Wurm Client> ERROR: Failed to set alignment updates");
                    }
                    if (config.SkillGainRate != WurmClient.Configs.EnumSkillGainRate.per0_001)
                    {
                        allChanges.Add(config.SetSkillGainRate(WurmClient.Configs.EnumSkillGainRate.per0_001)
                            ? "Wurm Client> Skillgain Tab Updates set to: Per 0.001 skill gain"
                            : "Wurm Client> ERROR: Failed to set Skillgain Tab Updates");
                    }
                }

                results.OnUpdateShowNewFeatures = true;
                allChanges.Add("Wurm Assistant> Notify about new features and major changes: Yes");
                results.OnUpdateShowFullChangelog = false;
                allChanges.Add("Wurm Assistant> Show changelog after every update: No");
            }
            else if (configMode_mode == ConfigMode_Modes.Manual)
            {
                WurmClient.Configs.ConfigData[] allconfigs = WurmClient.Configs.GetAllConfigs();

                foreach (var config in allconfigs)
                {
                    allChanges.Add("Reviewing Wurm Client config: " + config.ConfigName);

                    if (results.LoggingType != null)
                    {
                        if (config.SetCommonLoggingMode(results.LoggingType.Value))
                            allChanges.Add("Wurm Client> Event, IRC and Other message logging to: " + results.LoggingType.Value.ToString());
                        else allChanges.Add("Wurm Client> ERROR: Failed to set message logging");
                    }

                    if (results.TimestampMessages != null)
                    {
                        if (config.SetTimestampMessages(results.TimestampMessages.Value))
                            allChanges.Add("Wurm Client> Timestamp Messages set to: " + results.TimestampMessages.Value.ToString());
                        else allChanges.Add("Wurm Client> ERROR: Failed to set timestamp messages");
                    }

                    if (results.FavorAndAlignmentUpdates != null)
                    {
                        if (config.SetNoSkillMessageOnFavorChange(!results.FavorAndAlignmentUpdates.Value))
                            allChanges.Add("Wurm Client> Hide favor updates set to : " + (!results.FavorAndAlignmentUpdates.Value).ToString());
                        else allChanges.Add("Wurm Client> ERROR: Failed to set favor updates");

                        if (config.SetNoSkillMessageOnAlignmentChange(!results.FavorAndAlignmentUpdates.Value))
                            allChanges.Add("Wurm Client> Hide alignment updates set to : " + (!results.FavorAndAlignmentUpdates.Value).ToString());
                        else allChanges.Add("Wurm Client> ERROR: Failed to set alignment updates");
                    }

                    if (results.SkillGainRate != null)
                    {
                        if (config.SetSkillGainRate(results.SkillGainRate.Value))
                            allChanges.Add("Wurm Client> Skillgain Tab Updates set to: " + results.SkillGainRate.Value.ToString());
                        else allChanges.Add("Wurm Client> ERROR: Failed to set Skillgain Tab Updates");
                    }
                }

                allChanges.Add("Wurm Assistant> Notify about new features and major changes: "
                    + (results.OnUpdateShowNewFeatures == true ? "Yes" : "No"));
                allChanges.Add("Wurm Assistant> Show changelog after every update: "
                    + (results.OnUpdateShowFullChangelog == true ? "Yes" : "No"));
            }
            else throw new InvalidOperationException("ConfigMode can't be unset at this point in wizard");

            wizardCompleted = true;
            Logger.LogInfo("Wizard completed successfully", this);

            textBoxChangesSummary.Lines = allChanges.ToArray();
            tabControlWizard.SelectTab(tabPageFinished);
            Finish_buttonFinish.Enabled = true;
        }

        private void Finish_buttonFinish_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        private void FormConfigWizard_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (wizardCompleted)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            else this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void tabControlWizard_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void tabControlWizard_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void FormConfigWizard_Load(object sender, EventArgs e)
        {
            if (_parent != null && this.WindowState != FormWindowState.Minimized)
                this.Location = FormHelper.GetCenteredChildPositionRelativeToParentWorkAreaBound(this, _parent);
        }
    }
}
