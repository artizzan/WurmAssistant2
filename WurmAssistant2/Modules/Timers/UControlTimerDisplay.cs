using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility.WinFormsManagers;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Timers
{
    public partial class UControlTimerDisplay : UserControl
    {
        private WurmTimer WurmTimer;

        public UControlTimerDisplay()
        {
            InitializeComponent();
        }

        public UControlTimerDisplay(WurmTimer wurmTimer) : this()
        {
            this.WurmTimer = wurmTimer;
            SetName(wurmTimer.TimerShortID);
        }

        TimeSpan CooldownLength = TimeSpan.Zero;

        string TimerName = string.Empty;
        string SkillLevel = "0";
        private string MeditCount = "0";

        public void SetName(string text)
        {
            TimerName = text;
            labelName.Text = TimerName;
        }

        /// <summary>
        /// Sets the duration of a cooldown, as displayed on progress bar. If actual cooldown is longer, it will show as empty progress bar.
        /// </summary>
        /// <param name="cd_length"></param>
        public void SetCooldown(TimeSpan cd_length)
        {
            CooldownLength = cd_length;
        }

        public void UpdateCooldown(DateTime cooldownTo)
        {
            UpdateCooldown(cooldownTo - DateTime.Now);
        }

        /// <summary>
        /// update skill with most recent skill value
        /// </summary>
        /// <param name="skillValue"></param>
        public void UpdateSkill(float skillValue)
        {
            SkillLevel = skillValue.ToString("F2");
        }

        /// <summary>
        /// sets the skill display to any text
        /// </summary>
        /// <param name="txt"></param>
        public void SetCustomStringAsSkill(string txt)
        {
            SkillLevel = txt;
        }

        public void SetMeditCount(int count)
        {
            MeditCount = count.ToString();
        }

        /// <summary>
        /// show skill in () after timer name
        /// </summary>
        public bool ShowSkill = false;

        private WidgetModeManager _widgetManager;

        /// <summary>
        /// set to display extra info appended to remaining time,
        /// set null or empty to disable
        /// </summary>
        public string ExtraInfo { get; set; }

        public bool ShowMeditCount { get; set; }

        public WidgetModeManager WidgetManager
        {
            private get { return _widgetManager; }
            set 
            { 
                _widgetManager = value;
                //_widgetManager.WidgetModeChanging += _widgetManager_WidgetModeChanging; 
            }
        }

        void _widgetManager_WidgetModeChanging(object sender, WidgetModeEventArgs e)
        {
        }

        public void UpdateCooldown(TimeSpan cd_remaining)
        {
            string presentation = TimerName;
            if (ShowSkill) presentation += " ("+SkillLevel+")";
            if (ShowMeditCount) presentation += " " + MeditCount;
            labelName.Text = presentation;

            if (cd_remaining.Ticks < 0)
            {
                labelTimeTo.Text = "ready!";
                progressBar1.Value = progressBar1.Maximum;
            }
            else
            {
                int value = (int)((cd_remaining.TotalSeconds / CooldownLength.TotalSeconds) * progressBar1.Maximum);
                if (value > progressBar1.Maximum) value = progressBar1.Maximum;
                else if (value < 0) value = 0;

                value = progressBar1.Maximum - value;

                progressBar1.Value = value;

                labelTimeTo.Text = string.Empty;
                if (cd_remaining.Days > 1) labelTimeTo.Text += String.Format("{0} days ", cd_remaining.Days);
                else if (cd_remaining.Days > 0) labelTimeTo.Text += String.Format("{0} day ", cd_remaining.Days);
                labelTimeTo.Text += cd_remaining.ToString(@"hh\:mm\:ss");
                if (!string.IsNullOrEmpty(ExtraInfo)) labelTimeTo.Text += ExtraInfo;
            }
        }

        private void UControlTimerDisplay_MouseClick(object sender, MouseEventArgs e)
        {
            HandleMouseClick(e);
        }

        private void labelTimeTo_MouseClick(object sender, MouseEventArgs e)
        {
            HandleMouseClick(e);
        }

        private void labelName_MouseClick(object sender, MouseEventArgs e)
        {
            HandleMouseClick(e);
        }

        private void progressBar1_MouseClick(object sender, MouseEventArgs e)
        {
            HandleMouseClick(e);
        }

        void HandleMouseClick(MouseEventArgs e)
        {
            if (WidgetManager != null && WidgetManager.WidgetMode)
            {
                return;
            }
            if (e.Button == MouseButtons.Right) WurmTimer.OpenTimerConfig();
        }

        private void tableLayoutPanel2_MouseClick(object sender, MouseEventArgs e)
        {
            HandleMouseClick(e);
        }

        private void tableLayoutPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            HandleMouseClick(e);
        }
    }
}
