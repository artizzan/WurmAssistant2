using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Aldurcraft.Utility.Helpers;
using Aldurcraft.Utility.SoundEngine;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    public partial class FormTriggersConfig : Form
    {
        TriggerManager ParentModule;
        private const string DisplayName = "Triggers";

        public FormTriggersConfig(TriggerManager module)
        {
            InitializeComponent();
            this.ParentModule = module;
            BuildFormText();
            UpdateMutedState();
            TriggersListView.SetObjects(ParentModule.Settings.Value.Triggers);
            timer1.Enabled = true;
        }

        private void RefreshBankAndList()
        {
            SoundBank.RebuildSoundBank();
            TriggersListView.BuildList(true);
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            RefreshBankAndList();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            var ui = new TriggerChoice();
            if (ui.ShowDialogCenteredEx(this) == DialogResult.OK)
            {
                var trigger = ui.Result;
                trigger.MuteChecker = ParentModule.Settings.Value.GetMutedEvaluator();
                ParentModule.Settings.Value.AddTrigger(trigger);
                var ui2 = ui.Result.ShowAndGetEditUi(this); // new EditTrigger(ui.Result);
                ui2.Closed += (o, args) =>
                              {
                                  TriggersListView.BuildList(true);
                                  ParentModule.Settings.DelayedSave();
                              };
                //ui2.ShowCenteredEx(this);
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            EditCurrentItem();
        }

        void EditCurrentItem()
        {
            var selected = TriggersListView.SelectedObject;
            if (selected != null)
            {
                var ui = ((ITrigger)selected).ShowAndGetEditUi(this);
                ui.Closed -= EditTriggerClosed; //in case this is already hooked
                ui.Closed += EditTriggerClosed;
                //ui.ShowCenteredEx(this);
            }
        }

        private void EditTriggerClosed(object o, EventArgs args)
        {
            TriggersListView.BuildList(true);
            ParentModule.Settings.DelayedSave();
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            RemoveCurrentItem();
        }

        void RemoveCurrentItem()
        {
            var selected = TriggersListView.SelectedObject;
            if (selected != null)
            {
                ParentModule.Settings.Value.RemoveTrigger((ITrigger)selected);
                TriggersListView.BuildList(true);
                ParentModule.Settings.DelayedSave();
            }
        }

        private void FormSoundNotifyConfig_Load(object sender, EventArgs e)
        {
            RefreshBankAndList();
            //if (OperatingSystemInfo.RunningOS == OperatingSystemInfo.OStype.WinXP)
            //{
            //    this.Size = new Size(this.Size.Width + 190, this.Size.Height);
            //}
            TriggersListView.RestoreState(ParentModule.Settings.Value.TriggerListState);
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

                BuildFormText();
            }
            else
            {
                SoundBank.StopSounds();
                buttonMute.Image = Properties.Resources.SoundDisabledSmall;
                BuildFormText(true);
                this.Text = String.Format("{1} ({0}) [MUTED]", ParentModule.Player, DisplayName);
            }
            ParentModule.UpdateMutedState();
        }

        void BuildFormText(bool muted = false)
        {
            this.Text = String.Format("{1} ({0}){2}",
                ParentModule.Player,
                DisplayName,
                muted ? " [MUTED]" : "");
        }

        private void buttonManageSNDBank_Click(object sender, EventArgs e)
        {
            SoundBank.OpenSoundBank();
        }

        private void FormSoundNotifyConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }

            SaveStateToSettings();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.Visible) TriggersListView.BuildList(true);
        }

        private void FormTriggersConfig_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible) TriggersListView.BuildList(true);
        }

        private void TriggersListView_FormatRow(object sender, BrightIdeasSoftware.FormatRowEventArgs e)
        {
            var trigger = (ITrigger)e.Model;
            if (!trigger.Active) e.Item.BackColor = Color.LightGray;
        }

        private void TriggersListView_CellClick(object sender, BrightIdeasSoftware.CellClickEventArgs e)
        {
        }

        private void TriggersListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var selected = TriggersListView.SelectedObject;
                if (selected != null)
                {
                    var trigger = (ITrigger)selected;
                    trigger.Active = !trigger.Active;
                    TriggersListView.BuildList(true);
                    TriggersListView.DeselectAll();
                }
            }
        }

        private void TriggersListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                EditCurrentItem();
            }
        }

        void SaveStateToSettings()
        {
            var settings = TriggersListView.SaveState();
            ParentModule.Settings.Value.TriggerListState = settings;
            ParentModule.Settings.Save();
        }

        private void TriggersListView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveCurrentItem();
            }
        }

        private void TriggersListView_FormatCell(object sender, BrightIdeasSoftware.FormatCellEventArgs e)
        {
            if (e.Column == olvColumnSound)
            {
                var trigger = (ITrigger)e.Model;
                switch (trigger.HasSoundAspect)
                {
                    case ThreeStateBool.True:
                        e.SubItem.BackColor = Color.LightGreen;
                        break;
                    case ThreeStateBool.Error:
                        e.SubItem.BackColor = Color.PeachPuff;
                        break;
                }
            }
            else if (e.Column == olvColumnPopup)
            {
                var trigger = (ITrigger)e.Model;
                if (trigger.HasPopupAspect == ThreeStateBool.True)
                {
                    e.SubItem.BackColor = Color.LightGreen;
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {

        }
    }
}
