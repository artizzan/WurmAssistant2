using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Aldurcraft.Utility;

namespace WA2_Test
{
    public partial class Logger_Test : Form
    {
        string curDir = Path.GetDirectoryName(Application.ExecutablePath);

        public Logger_Test()
        {
            InitializeComponent();
            Logger.SetLogSaveDir(curDir);
        }

        private void Logger_Test_Load(object sender, EventArgs e)
        {
            listBoxPriority.Items.AddRange(Logger.GetPriorities());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Logger.LogMessagePriority priority; 
            if (listBoxPriority.SelectedIndex >= 0)
            {
                priority = (Logger.LogMessagePriority)Enum.Parse(typeof(Logger.LogMessagePriority), listBoxPriority.SelectedItem.ToString());
            }
            Logger.Log(Logger.LogMessagePriority.Info,
                textBoxMessage.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Logger.OpenLogViewer();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string indxoutofbounds = "123";
            try
            {
                char test = indxoutofbounds[10];
            }
            catch (Exception _e)
            {
                Logger.LogError("", this, _e);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Logger.Log(Logger.LogMessagePriority.Info, "log message");
            Logger.Log(Logger.LogMessagePriority.Error, "log message");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            throw new Exception("unhandled");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //Logger.SaveLogsToXML(Path.Combine(curDir, "logs.XML"));
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Logger.SetTBOutput(textBoxLog);
        }

        private void textBoxLog_TextChanged(object sender, EventArgs e)
        {
            textBoxLog.SelectionStart = textBoxLog.Text.Length;
            textBoxLog.ScrollToCaret();
        }
    }
}
