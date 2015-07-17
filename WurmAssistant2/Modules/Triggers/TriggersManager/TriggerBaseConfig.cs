using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Ex;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility.Helpers;
using Aldurcraft.WurmOnline.WurmLogsManager;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    public partial class TriggerBaseConfig : UserControl, ITriggerConfig
    {
        private readonly TriggerBase _trigger;
        private bool _initComplete = false;
        public TriggerBaseConfig(TriggerBase trigger)
        {
            _trigger = trigger;
            InitializeComponent();
            toolTip1.SetToolTip(ResetOnCndHitChkbox, "Every time condition is hit, cooldown will be set to full duration (including when trigger is already on cooldown). Useful for new chat message triggers.".GetLineWrappedStringEx());

            TriggerNameTbox.Text = trigger.Name;
            ApplicableLogsDisplayTxtbox.Text = trigger.LogTypesAspect;
            foreach (var logType in GameLogTypesEX.GetAllLogTypes())
            {
                LogTypesChklist.Items.Add(logType, trigger.CheckLogType(logType));
            }
            if (trigger.LogTypesLocked) LogTypesChklist.Enabled = false;
            CooldownEnabledChkbox.Checked
                = ResetOnCndHitChkbox.Enabled
                = CooldownInput.Enabled 
                = trigger.CooldownEnabled;
            CooldownInput.Value = trigger.Cooldown;
            ResetOnCndHitChkbox.Checked = _trigger.ResetOnConditonHit;
            if (_trigger.DefaultDelayFunctionalityDisabled)
            {
                DelayChkbox.Enabled = DelayInput.Enabled = false;
            }
            else
            {
                DelayChkbox.Checked = DelayInput.Enabled = _trigger.DelayEnabled;
                DelayInput.Value = _trigger.Delay;
            }

            _initComplete = true;
        }

        public UserControl ControlHandle { get { return this; } }

        private void TriggerNameTbox_TextChanged(object sender, EventArgs e)
        {
            if (_initComplete) _trigger.Name = TriggerNameTbox.Text;
        }

        private void LogTypesChklist_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_initComplete)
            {
                _trigger.SetLogType((GameLogTypes)LogTypesChklist.Items[e.Index], e.NewValue);
                ApplicableLogsDisplayTxtbox.Text = _trigger.LogTypesAspect;
            }
        }

        private void CooldownEnabledChkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (_initComplete)
            {
                CooldownInput.Enabled 
                    = ResetOnCndHitChkbox.Enabled
                    = _trigger.CooldownEnabled 
                    = CooldownEnabledChkbox.Checked;
            }
        }

        private void CooldownInput_ValueChanged(object sender, EventArgs e)
        {
            if (_initComplete) _trigger.Cooldown = CooldownInput.Value;
        }

        private void ResetOnCndHitChkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (_initComplete) _trigger.ResetOnConditonHit = ResetOnCndHitChkbox.Checked;
        }

        private void DelayChkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (_initComplete)
            {
                DelayInput.Enabled
                    = _trigger.DelayEnabled
                    = DelayChkbox.Checked;
            }
        }

        private void DelayInput_ValueChanged(object sender, EventArgs e)
        {
            if (_initComplete) _trigger.Delay = DelayInput.Value;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AppRun.RunLink(
                @"http://forum.wurmonline.com/index.php?/topic/68031-wurm-assistant-2x-bundle-of-useful-tools/#entry948073");
        }
    }
}
