using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.WurmOnline.WurmState;
using Aldurcraft.Utility;
using Aldurcraft.Utility.WinFormsManagers;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Timers
{
    public partial class FormTimers : Form
    {
        private ModuleTimers ParentModule;
        private Dictionary<string, int> PlayerToListBoxIndexMap = new Dictionary<string, int>();
        private bool _formInited = false;
        private WidgetModeManager _widgetManager;

        public FormTimers()
        {
            InitializeComponent();
            _widgetManager = new WidgetModeManager(this);
            _widgetManager.WidgetModeChanging += (sender, args) =>
                                                 {
                                                     buttonAddRemoveChars.Visible
                                                         = buttonCustomTimers.Visible
                                                             = buttonOptions.Visible
                                                                 = label1.Visible
                                                                     = !args.WidgetMode;
                                                 };
        }

        public FormTimers(ModuleTimers moduleTimers)
            : this()
        {
            this.ParentModule = moduleTimers;
        }

        private void FormTimers_Load(object sender, EventArgs e)
        {
            if (this.Visible) this.Size = new Size(ParentModule.Settings.Value.SavedWindowSize);
            try
            {
                if (panel1.Visible) panel1.Visible = false;
                string[] players = WurmClient.WurmPaths.GetAllPlayersNames();

                int index = 0;
                foreach (var player in players)
                {
                    checkedListBoxPlayers.Items.Add(player);
                    PlayerToListBoxIndexMap.Add(player, index);
                    index++;
                }

                UpdateSelectedPlayers();

                _widgetManager.Set(ParentModule.Settings.Value.WidgetModeEnabled);

                _formInited = true;
            }
            catch (Exception _e)
            {
                Logger.LogCritical("form load error", this, _e);
            }
        }

        private void UpdateSelectedPlayers()
        {
            try
            {
                var selectedPlayers = ParentModule.GetActivePlayerGroups();
                for (int i = 0; i < checkedListBoxPlayers.Items.Count; i++)
                {
                    object item = checkedListBoxPlayers.Items[i];
                    try
                    {
                        if (selectedPlayers.Contains(item.ToString()))
                        {
                            checkedListBoxPlayers.SetItemChecked(PlayerToListBoxIndexMap[item.ToString()], true);
                        }
                        else
                        {
                            checkedListBoxPlayers.SetItemChecked(PlayerToListBoxIndexMap[item.ToString()], false);
                        }
                    }
                    catch (Exception _e)
                    {
                        Logger.LogError("problem updating player list", this, _e);
                    }
                }
            }
            catch (Exception _e)
            {
                Logger.LogCritical("form load error", this, _e);
            }
        }

        private void checkedListBoxPlayers_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_formInited)
            {
                string player = checkedListBoxPlayers.Items[e.Index].ToString();
                if (e.NewValue == CheckState.Checked)
                {
                    ParentModule.AddNewPlayerGroup(player);
                }
                else
                {
                    ParentModule.RemovePlayerGroup(player);
                }
            }
        }

        private void FormTimers_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        public void RestoreFromMin()
        {
            if (this.WindowState == FormWindowState.Minimized) this.WindowState = FormWindowState.Normal;
        }

        internal void RegisterTimersGroup(UControlPlayerLayout layoutControl)
        {
            layoutControl.WidgetManager = _widgetManager;
            flowLayoutPanel1.Controls.Add(layoutControl);
            _widgetManager.ResetMouseEvents();
        }

        internal void UnregisterTimersGroup(UControlPlayerLayout layoutControl)
        {
            flowLayoutPanel1.Controls.Remove(layoutControl);
            _widgetManager.ResetMouseEvents();
        }

        private void buttonAddRemoveChars_Click(object sender, EventArgs e)
        {
            panel1.Visible = !panel1.Visible;
        }

        private void checkedListBoxPlayers_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private CustomTimersManager CustomTimersManagerUI = null;

        private void buttonCustomTimers_Click(object sender, EventArgs e)
        {
            try
            {
                if (CustomTimersManagerUI.WindowState == FormWindowState.Minimized)
                    CustomTimersManagerUI.WindowState = FormWindowState.Normal;
                CustomTimersManagerUI.Show();
                CustomTimersManagerUI.BringToFront();
            }
            catch
            {
                CustomTimersManagerUI = new CustomTimersManager(this);
                CustomTimersManagerUI.Show();
            }
        }

        private void FormTimers_Resize(object sender, EventArgs e)
        {
            if (_formInited)
            {
                ParentModule.Settings.Value.SavedWindowSize = new Point(this.Size.Width, this.Size.Height);
                ParentModule.Settings.DelayedSave();
            }
        }

        private void buttonOptions_Click(object sender, EventArgs e)
        {
            var ui = new FormTimerGlobalSettings(this);
            ui.ShowDialog();
        }

        public bool WidgetModeEnabled
        {
            get { return ParentModule.Settings.Value.WidgetModeEnabled; }
            set
            {
                ParentModule.Settings.Value.WidgetModeEnabled = value;
                ParentModule.Settings.DelayedSave();
                _widgetManager.Set(value);
            }
        }

        public Color WidgetBgColor
        {
            get { return ParentModule.Settings.Value.WidgetBgColor; }
            set
            {
                ParentModule.Settings.Value.WidgetBgColor = value;
                ParentModule.Settings.DelayedSave();
            }
        }

        public Color WidgetForeColor
        {
            get { return ParentModule.Settings.Value.WidgetForeColor; }
            set
            {
                ParentModule.Settings.Value.WidgetForeColor = value;
                ParentModule.Settings.DelayedSave();
            }
        }
    }
}
