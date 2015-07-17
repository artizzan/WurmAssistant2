using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger
{
    public partial class FormChooseHerd : Form
    {
        private GrangerContext Context;
        private UCGrangerHorseList ControlGrangerHorseList;
        private FormGrangerMain MainForm;
        public string Result
        {
            get
            {
                if (listBox1.SelectedItem == null) return null;
                return listBox1.SelectedItem.ToString();
            }
        }

        public FormChooseHerd(FormGrangerMain mainForm, GrangerContext Context)
        {
            this.MainForm = mainForm;
            this.Context = Context;
            InitializeComponent();

            var herds = Context.Herds.ToArray();

            listBox1.Items.AddRange(herds);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                buttonOK.Enabled = true;
            }
            else
            {
                buttonOK.Enabled = false;
            }
        }

        private void FormChooseHerd_Load(object sender, EventArgs e)
        {
            this.Location = FormHelper.GetCenteredChildPositionRelativeToParentWorkAreaBound(this, MainForm);
        }


    }
}
