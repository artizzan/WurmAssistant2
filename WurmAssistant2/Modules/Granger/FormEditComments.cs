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
    public partial class FormEditComments : Form
    {
        private string HorseComments;
        private FormGrangerMain formGrangerMain;

        public FormEditComments(FormGrangerMain formGrangerMain, string horseComments, string horseName)
        {
            this.formGrangerMain = formGrangerMain;
            this.HorseComments = horseComments;
            InitializeComponent();
            this.Text = "Edit comments for: " + horseName;
            this.textBox1.Text = HorseComments;
            textBox1.Select();
        }

        public string Result
        {
            get
            {
                if (this.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    return textBox1.Text;
                }
                else throw new InvalidOperationException("Dialog result not OK");
            }
        }

        private void FormEditComments_Load(object sender, EventArgs e)
        {
            this.Location = FormHelper.GetCenteredChildPositionRelativeToParentWorkAreaBound(this, formGrangerMain);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                buttonOK.PerformClick();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                buttonCancel.PerformClick();
            }
        }
    }
}
