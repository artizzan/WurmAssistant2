using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using Aldurcraft.Utility;
using Aldurcraft.Utility.SoundEngine;
using Aldurcraft.WurmOnline.WurmAssistant2.DebugWindows;
using Aldurcraft.WurmOnline.WurmAssistant2.Engine;
using Aldurcraft.WurmOnline.WurmLogsManager.Searcher;

namespace Aldurcraft.WurmOnline.WurmAssistant2
{
    public partial class WurmAssistant : Form
    {
        private TrayContextMenuManager _contextMenuManager;

        public WurmAssistant()
        {
            InitializeComponent();
            EnableDebagButton(); //only in debag mode!
            if (DateTime.Now > new DateTime(2015, 1, 31).AddDays(14))
            {
                linkLabelAssistantFuture.Visible = false;
            }
            _contextMenuManager = new TrayContextMenuManager(this, contextMenuStrip1);
        }

        [Conditional("DEBUG")]
        private void EnableDebagButton()
        {
            buttonDebag.Enabled = buttonDebag.Visible = true;
        }

        bool _initCompleted = false;
        private void WurmAssistant2_Load(object sender, EventArgs e)
        {
            try
            {
                Version assistantVersion = Assembly.GetEntryAssembly().GetName().Version;
                this.Text += String.Format(" ({0})", assistantVersion.ToString());
                if (!AssistantEngine.Init1_Settings(this))
                {
                    MessageBox.Show("Wurm Assistant has closed because configuration process was not completed.", "Note", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    Application.Exit();
                }

                toolTip1.SetToolTip(this.buttonBuyBeerYarr, "Fuel our work!");

                // disabled due issues with window starting offscreen
                //this.Location = AssistantEngine.Settings.Value.WindowLocation;
                this.Size = new System.Drawing.Size(AssistantEngine.Settings.Value.WindowSize);
                if (AssistantEngine.Settings.Value.StartMinimized)
                {
                    this.WindowState = FormWindowState.Minimized;
                }

                AssistantEngine.Init2_Engine();

                _initCompleted = true;
                EnableUpdate = true;

                timerInit.Enabled = true;
            }
            catch (Exception _e)
            {
                Logger.LogCritical("Exception at LOAD", this, _e);
                Application.Exit();
            }
        }

        private bool _timerInitCompleted = false;
        private void timerInit_Tick(object sender, EventArgs e)
        {
            timerInit.Enabled = false;
            // why on earth would this fire a few times, when timerInit.Enabled=false is at the end and sometimes even when at the top??
            if (!_timerInitCompleted)
            {
                _timerInitCompleted = true;

                // bugged: appears on taskbar again
                if (this.WindowState == FormWindowState.Minimized) this.Hide();
                AlwaysShowNotifyIconIfSet();

                AssistantEngine.AfterInit();
            }
        }

        public TextBox GetTextBoxForLog()
        {
            return textBoxLog;
        }

        internal bool EnableUpdate
        {
            get { return timerUpdateLoop.Enabled; }
            set { timerUpdateLoop.Enabled = value; }
        }

        private void timerUpdateLoop_Tick(object sender, EventArgs e)
        {
            AssistantEngine.Update();

            string input = AssistantEngine.ErrorCounter.GetUpdate();
            if (input != null)
            {
                textBoxStatusWindow.Text = input;
            }
        }

        #region PROGRAM LOG

        bool logPanelExpanded = false;

        private void buttonExpandLog_Click(object sender, EventArgs e)
        {
            const int resizeval = 200;
            bool flag = false;
            if (!logPanelExpanded)
            {
                panelLog.Top = panelLog.Top - resizeval;
                panelLog.Size = new System.Drawing.Size(panelLog.Size.Width, panelLog.Size.Height + resizeval);
                flag = true;
            }
            if (logPanelExpanded)
            {
                panelLog.Top = panelLog.Top + resizeval;
                panelLog.Size = new System.Drawing.Size(panelLog.Size.Width, panelLog.Size.Height - resizeval);
                flag = false;
            }
            logPanelExpanded = flag;
        }

        #endregion

        private void WurmAssistant2_Resize(object sender, EventArgs e)
        {
            HandleMinimizeToTray();
            TextBoxScrollToBottom();
            //adjust panels counts and properties
            if (_initCompleted) AddNewSizeAndPosToSettings();
        }

        private void WurmAssistant2_Move(object sender, EventArgs e)
        {
            if (_initCompleted) AddNewSizeAndPosToSettings();
        }

        void AddNewSizeAndPosToSettings()
        {
            AssistantEngine.Settings.Value.WindowSize = new Point(this.Size.Width, this.Size.Height);
            AssistantEngine.Settings.Value.WindowLocation = this.Location;
            AssistantEngine.Settings.DelayedSave();
        }

        private void WurmAssistant2_Paint(object sender, PaintEventArgs e)
        {
            panelLog.BringToFront();
        }

        private void tableLayoutPanelModuleUI_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBoxLog_TextChanged(object sender, EventArgs e)
        {
            TextBoxScrollToBottom();
        }

        private void TextBoxScrollToBottom()
        {
            // autoscroll to bottom
            textBoxLog.SelectionStart = textBoxLog.Text.Length;
            textBoxLog.ScrollToCaret();
        }

        private void WurmAssistant2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (AssistantEngine.Settings.Value.PromptOnExit && e.CloseReason == CloseReason.UserClosing)
            {
                if (MessageBox.Show("Exit Wurm Assistant 2?", "Confirm", MessageBoxButtons.YesNo) ==
                    DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            ShowRestore(new object(), EventArgs.Empty);
            AssistantEngine.AppClosing();
        }

        private void modulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AssistantEngine.Modules.ConfigureModules();
        }

        #region MODULE LAYOUT PANEL AND TRAY MENU

        Dictionary<Type, Button> ModuleButtons = new Dictionary<Type, Button>();

        internal void AddModuleButton(AssistantModule module)
        {
            var descriptor = AssistantModuleDescriptors.GetDescriptor(module.GetType());

            if (ModuleButtons.ContainsKey(descriptor.ModuleType))
                throw new InvalidOperationException("button already exists in the layout");

            Button btn = new Button();
            btn.Size = new Size(80, 80);
            if (descriptor.IconPath != null)
            {
                try
                {
                    btn.BackgroundImageLayout = ImageLayout.Stretch;
                    btn.BackgroundImage = Image.FromFile(descriptor.IconPath);
                }
                catch (Exception _e)
                {
                    Logger.LogError("problem loading module icon", this, _e);
                }
            }
            btn.Click += module.OpenUI;
            toolTip1.SetToolTip(btn, descriptor.Name);
            flowLayoutPanelModules.Controls.Add(btn);

            ModuleButtons.Add(descriptor.ModuleType, btn);

            _contextMenuManager.Rebuild();
        }

        internal void RemoveModuleButton(Type type)
        {
            try
            {
                Button btn = ModuleButtons[type];
                flowLayoutPanelModules.Controls.Remove(btn);
                ModuleButtons.Remove(type);
            }
            catch (KeyNotFoundException)
            {
                Logger.LogError("button remove request did not find any button to remove, type: " + type.ToString(), this);
            }

            _contextMenuManager.Rebuild();
        }

        #endregion

        private void buttonOpenCurrentLog_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Logger.LogSavePath);
            }
            catch (Exception ex)
            {
                Logger.LogError("Problem opening current log file: " + (Logger.LogSavePath ?? "NULL"), this, ex);
            }
        }

        private void openLogDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AssistantEngine.OpenLogDir();
        }

        private void buttonLogDir_Click(object sender, EventArgs e)
        {
            //AssistantEngine.OpenForumThread();
            AssistantEngine.OpenLogDir();
        }

        private void buttonSendFeedback_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("mailto:aldurcraft@gmail.com");
            }
            catch (Exception ex)
            {
                Logger.LogError("problem on feedback button", this, ex);
            }
        }

        private void soundsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SoundBank.OpenSoundBank();
        }

        private void changeSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSettings ui = new FormSettings();
            ui.Show();
            if (this.WindowState != FormWindowState.Minimized)
                ui.Location = FormHelper.GetCenteredChildPositionRelativeToParentWorkAreaBound(ui, this);
        }

        private void configurationWizardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormConfigWizard ui = new FormConfigWizard(this);

            if (ui.ShowDialog() == DialogResult.OK)
            {
                AssistantEngine.ApplyWizardResults(ui.results);
                AssistantEngine.ScheduleSearcherDbWipeOnNextRun();
                MessageBox.Show("Wurm Assistant must restart now");
                Application.Restart();
            }
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(@"http://wurmassistant.wikia.com/");
            }
            catch (Exception _e)
            {
                Logger.LogError("problem opening wiki page?", this, _e);
            }
        }

        private void buttonBuyBeerYarr_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(@"http://blog.aldurcraft.com/WurmAssistant/page/Buy-me-a-beer");
            }
            catch (Exception _e)
            {
                Logger.LogError("Problem opening donation link", this, _e);
            }
        }

        public bool ButtonBuyBeerHidden
        {
            get { return !buttonBuyBeerYarr.Visible; }
            set { buttonBuyBeerYarr.Visible = !value; }
        }

        private void creditsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(@"http://blog.aldurcraft.com/WurmAssistant/page/Contributors-and-Supporters");
            }
            catch (Exception _e)
            {
                Logger.LogError("Problem opening contributor link", this, _e);
            }
        }

        private void contributorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //removed
        }

        //no longer to blog, back to official thread
        private void blogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(@"http://forum.wurmonline.com/index.php?/topic/68031-wurm-assistant-2x-bundle-of-useful-tools/");
            }
            catch (Exception _e)
            {
                Logger.LogError("Problem opening blog link", this, _e);
            }
        }

        private void throwToolStripMenuItem_Click(object sender, EventArgs e)
        {
            numericUpDown1.Value = 101;
        }

        private void HandleMinimizeToTray()
        {
            if (AssistantEngine.Settings.Value.MiminizeToTray)
            {
                if (FormWindowState.Minimized == this.WindowState)
                {
                    notifyIcon1.Visible = true;
                    if (!AssistantEngine.Settings.Value.BallonTooltipShown)
                    {
                        notifyIcon1.ShowBalloonTip(5000);
                        AssistantEngine.Settings.Value.BallonTooltipShown = true;
                    }
                    this.Hide();
                }
                else if (FormWindowState.Normal == this.WindowState)
                {
                    notifyIcon1.Visible = false;
                }
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                ShowRestore(new object(), EventArgs.Empty);
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                contextMenuStrip1.Show();
            }
        }

        internal void ShowRestore(object sender, EventArgs e)
        {
            this.Show();
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            AlwaysShowNotifyIconIfSet();
        }

        internal void CloseConfirm(object sender, EventArgs e)
        {
            this.Close();
        }

        private void AlwaysShowNotifyIconIfSet()
        {
            if (AssistantEngine.Settings.Value.AlwaysShowNotifyIcon) notifyIcon1.Visible = true;
        }

        private void buttonDebag_Click(object sender, EventArgs e)
        {
            var ui = new DebugMain();
            ui.Show();
        }

        private void rdPartyToolsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void rebuildLogsCacheToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WurmLogSearcherAPI.TryScheduleForceRecache();
        }

        private void roadmapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(@"https://trello.com/b/Wl58d6PR/wurm-assistant");
            }
            catch (Exception _e)
            {
                Logger.LogError("Problem opening roadmap link", this, _e);
            }
        }

        private void linkLabelDownloadNewLauncher_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(@"http://old.aldurcraft.com/wurmassistant/download");
            }
            catch (Exception _e)
            {
                Logger.LogError("Problem opening linkLabelDownloadNewLauncher link", this, _e);
            }
        }

        private void linkLabelAssistantFuture_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(@"https://www.surveymonkey.com/s/QNRZSQF");
            }
            catch (Exception _e)
            {
                Logger.LogError("Problem at linkLabelAssistantFuture_LinkClicked", this, _e);
            }
        }

    }
}
