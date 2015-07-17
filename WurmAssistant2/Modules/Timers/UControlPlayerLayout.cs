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
    public partial class UControlPlayerLayout : UserControl
    {
        private PlayerTimersGroup ParentGroup;
        private WidgetModeManager _widgetManager;

        public UControlPlayerLayout()
        {
            InitializeComponent();
            //this.BackColor = DefaultBackColor;
        }

        public UControlPlayerLayout(PlayerTimersGroup playerTimersGroup)
            : this()
        {
            this.ParentGroup = playerTimersGroup;
            this.label1.Text = ParentGroup.Player + " (conjuring, please wait)";
        }

        public WidgetModeManager WidgetManager
        {
            private get { return _widgetManager; }
            set
            {
                _widgetManager = value;
                _widgetManager.WidgetModeChanging += _widgetManager_WidgetModeChanging;
            }
        }

        void _widgetManager_WidgetModeChanging(object sender, WidgetModeEventArgs e)
        {
            buttonAdd.Visible = !e.WidgetMode;
            if (e.WidgetMode)
            {
                this.BackColor = ParentGroup.GlobalSettings.Value.WidgetBgColor;
                this.ForeColor = ParentGroup.GlobalSettings.Value.WidgetForeColor;
            }
            else
            {
                this.BackColor = DefaultBackColor;
                this.ForeColor = DefaultForeColor;
            }
        }

        private void UControlPlayerLayout_Load(object sender, EventArgs e)
        {
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            ParentGroup.AddNewTimer();
        }

        internal void RegisterNewTimerDisplay(UControlTimerDisplay ControlTimer)
        {
            ControlTimer.WidgetManager = WidgetManager;
            this.flowLayoutPanel1.Controls.Add(ControlTimer);
            if (WidgetManager != null) WidgetManager.ResetMouseEvents();
        }

        internal void UnregisterTimerDisplay(UControlTimerDisplay ControlTimer)
        {
            this.flowLayoutPanel1.Controls.Remove(ControlTimer);
        }

        internal void EnableAddingTimers()
        {
            this.label1.Text = ParentGroup.Player;
            buttonAdd.Enabled = true;
        }
    }
}
