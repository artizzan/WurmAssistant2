using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Aldurcraft.WurmOnline.WurmLogsManager;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Timers
{
    public partial class CustomTimersManagerEditWindow : Form
    {
        string EditingNameID = null;
        Form parentForm;

        public CustomTimersManagerEditWindow(Form parent)
        {
            this.parentForm = parent;
            InitializeComponent();
            foreach (var type in GameLogTypesEX.GetAllLogTypes())
                comboBoxLogType.Items.Add(type);
            comboBoxLogType.SelectedItem = GameLogTypes.Event;
        }

        public CustomTimersManagerEditWindow(Form parent, string nameID)
            : this(parent)
        {

            EditingNameID = nameID;
            WurmTimerDescriptors.CustomTimerOptions options = WurmTimerDescriptors.GetOptionsForTimer(nameID);
            textBoxNameID.Text = nameID;
            textBoxNameID.Enabled = false;
            if (options.TriggerConditions != null)
            {
                textBoxCond.Text = options.TriggerConditions[0].RegexPattern;
                if (!options.IsRegex)
                {
                    textBoxCond.Text = Regex.Unescape(textBoxCond.Text);
                }
                else checkBoxAsRegex.Checked = true;
                comboBoxLogType.SelectedItem = options.TriggerConditions[0].LogType;
            }
            if (options.Duration != null) timeInputUControl2.Value = options.Duration;
            checkBoxUptimeReset.Checked = options.ResetOnUptime;
        }

        private void CustomTimersManagerEditWindow_Load(object sender, EventArgs e)
        {
            if (this.Visible) this.Location = FormHelper.GetCenteredChildPositionRelativeToParentWorkAreaBound(this, parentForm);
            toolTip1.SetToolTip(textBoxCond, "if not used as Regex, timer will start if this text is found in chosen log");
            toolTip1.SetToolTip(checkBoxAsRegex, "tip: use Log Searcher to test your Regex patterns.\r\nRegex pattern is raw and thus CASE-SENSITIVE (same as in Log Searcher)");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //validate
            if (IsValidData())
            {
                WurmTimerDescriptors.CustomTimerOptions options = new WurmTimerDescriptors.CustomTimerOptions();
                options.AddTrigger(textBoxCond.Text, (GameLogTypes)comboBoxLogType.SelectedItem, checkBoxAsRegex.Checked);
                options.Duration = timeInputUControl2.Value;
                options.ResetOnUptime = checkBoxUptimeReset.Checked;
                if (EditingNameID != null)
                {
                    WurmTimerDescriptors.RemoveCustomTimer(EditingNameID);
                }
                WurmTimerDescriptors.AddCustomTimer(textBoxNameID.Text, options);
                this.Close();
            }
        }

        bool IsValidData()
        {
            bool valid = true;
            if (textBoxNameID.Text.Trim() == string.Empty)
            {
                valid = false;
                MessageBox.Show("Timer name cannot be empty");
            }
            else if (EditingNameID == null && !WurmTimerDescriptors.IsThisNameIDUnique(textBoxNameID.Text))
            {
                valid = false;
                MessageBox.Show("Timer with this name already exists");
            }
            return valid;
        }
    }
}
