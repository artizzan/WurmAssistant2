using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Aldurcraft.WurmOnline.WurmAssistant2.Engine
{
    public partial class Wa3PromoForm : Form
    {
        public Wa3PromoForm()
        {
            InitializeComponent();
        }

        private void HelpWithWaLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            StartProcess("http://aldurslab.net/wurm-assistant-3-alpha/");
        }

        private void WurmApiLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            StartProcess("http://aldurslab.net/wurmapi/");
        }

        private void StartProcess(string process)
        {
            try
            {
                Process.Start(process);
            }
            catch (Exception exception)
            {
                MessageBox.Show(string.Format("Error trying to start process:{0}{1}{1}exception:{1}{2}",
                    process,
                    Environment.NewLine,
                    exception));
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        void Wa3PromoForm_Load(object sender, EventArgs e)
        {
        }

        private void downloadWa3Btn_Click(object sender, EventArgs e)
        {
            StartProcess("http://forum.wurmonline.com/index.php?/topic/68031-wurm-assistant-enrich-your-wurm-experience/");
        }

        private void downloadWauBtn_Click(object sender, EventArgs e)
        {
            StartProcess("http://forum.wurmonline.com/index.php?/topic/133948-wurm-assistant-for-wurm-unlimited-beta/");
        }

        private void Wa3BetaLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            StartProcess("http://aldurslab.net/wurm-assistant-3-beta/");
        }
    }
}
