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
    public partial class CustomTimersManager : Form
    {
        Form parentForm;
        public CustomTimersManager(Form parent)
        {
            parentForm = parent;
            InitializeComponent();
            ReloadList();
        }

        void ReloadList()
        {
            listBox1.Items.Clear();
            var customtimers = WurmTimerDescriptors.GetCustomTimers();
            foreach (var timer in customtimers)
            {
                listBox1.Items.Add(timer);
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            //display window to add
            CustomTimersManagerEditWindow ui = new CustomTimersManagerEditWindow(this);
            ui.Show();
            ui.FormClosed += OnTimerAdded;
        }

        private void OnTimerAdded(object sender, EventArgs e)
        {
            ReloadList();
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex > -1)
            {
                CustomTimersManagerEditWindow ui = new CustomTimersManagerEditWindow(this, listBox1.SelectedItem.ToString());
                ui.FormClosed += OnTimerAdded;
                ui.Show();
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            //remove timer
            if (listBox1.SelectedIndex > -1)
                WurmTimerDescriptors.RemoveCustomTimer(listBox1.SelectedItem.ToString());
            ReloadList();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void CustomTimersManager_Load(object sender, EventArgs e)
        {
            if (this.Visible) this.Location = FormHelper.GetCenteredChildPositionRelativeToParentWorkAreaBound(this, parentForm);
        }
    }
}
