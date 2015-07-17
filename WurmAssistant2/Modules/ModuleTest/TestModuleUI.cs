using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Test
{
    public partial class TestModuleUI : Form
    {
        TestModule ParentModule;
        public TestModuleUI(TestModule parentModule)
        {
            this.ParentModule = parentModule;
            InitializeComponent();
        }

        internal void ShowEvent(string entry)
        {
            textBox1.Text += entry + "\r\n";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ParentModule.Subscribe(textBox2.Text);
        }
    }
}
