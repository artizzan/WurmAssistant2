using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Timers
{
    public partial class FormVerifyAlignment : Form
    {
        string[] AllAlignments = null;

        public FormVerifyAlignment(string[] allalignments)
        {
            InitializeComponent();
            this.AllAlignments = allalignments;
        }

        private void FormVerifyAlignment_Load(object sender, EventArgs e)
        {
            listBox1.Items.Add("list as of date: " + DateTime.Now.ToString());

            if (AllAlignments != null) listBox1.Items.AddRange(AllAlignments); 
            else listBox1.Items.Add("no data available");
        }
    }
}
