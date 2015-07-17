using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Timers
{
    public partial class FormChooseTimers : Form
    {
        public FormChooseTimers()
        {
            InitializeComponent();
        }

        FormTimers parentForm;

        public FormChooseTimers(HashSet<WurmTimerDescriptors.TimerType> availableTypes, FormTimers parent)
            : this()
        {
            parentForm = parent;
            foreach (var type in availableTypes)
            {
                checkedListBox1.Items.Add(type);
            }
        }

        public HashSet<WurmTimerDescriptors.TimerType> Result = new HashSet<WurmTimerDescriptors.TimerType>();

        private void button1_Click(object sender, EventArgs e)
        {
            Result = new HashSet<WurmTimerDescriptors.TimerType>();
            foreach (var item in checkedListBox1.CheckedItems)
            {
                Result.Add((WurmTimerDescriptors.TimerType)item);
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void FormChooseTimers_Load(object sender, EventArgs e)
        {
            if (this.Visible) this.Location = FormHelper.GetCenteredChildPositionRelativeToParentWorkAreaBound(this, parentForm);
        }
    }
}
