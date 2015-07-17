using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.WurmOnline.WurmState;

namespace Aldurcraft.WurmOnline.WurmAssistant2.DebugWindows
{
    public partial class DebugMain : Form
    {
        public DebugMain()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var result = await PlayerServerTracker.GetServerNameForGroup("Somedur", WurmServer.ServerInfo.ServerGroup.Freedom);
                textBox1.Text = result.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                var result = await PlayerServerTracker.GetServerNameForGroup("Somedur", WurmServer.ServerInfo.ServerGroup.Epic);
                textBox1.Text = result.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
