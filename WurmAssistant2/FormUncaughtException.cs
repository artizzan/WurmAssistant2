using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Aldurcraft.WurmOnline.WurmAssistant2
{
    public partial class FormUncaughtException : Form
    {
        public FormUncaughtException(Exception e, string[] extraInfo)
        {
            InitializeComponent();

            string output = "";

            output = string.Format("EXCEPTION: {0}\r\nSOURCE:\r\n{1}\r\nTRACE:\r\n{2}\r\n",
                e.Message, e.Source, e.StackTrace);

            if (extraInfo != null && extraInfo.Length > 0)
            {
                output += string.Format("EXTRA INFORMATION:\r\n * {0}",
                    string.Join("\r\n * ", extraInfo));
            }

            textBox1.Text = output;
        }

        private void FormUncaughtException_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.BringToFront();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AssistantEngine.OpenForumThread();
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            textBox1.Select(0, textBox1.TextLength);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AssistantEngine.OpenLogDir();
        }
    }
}
