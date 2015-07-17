using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.WurmOnline.WurmState;
using System.Threading.Tasks;

namespace WA2_Test
{
    public partial class ServerDataTest : Form
    {
        public ServerDataTest()
        {
            InitializeComponent();
        }

        private void ServerDataTest_Load(object sender, EventArgs e)
        {
            WurmServer.InitializeWurmServer();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ShowUptime();
        }

        async Task ShowUptime()
        {
            try
            {
                TimeSpan? uptime;
                uptime = await WurmServer.GetUptimeAsync(textBox2.Text);
                textBox1.Text = uptime.ToString();
                WurmDateTime? wdt = await WurmServer.GetWurmDateTimeAsync(textBox2.Text);
                textBox1.Text += "\r\n" + wdt;
            }
            catch (Exception _e)
            {
                MessageBox.Show(_e.Message);
            }
        }

        WurmDateTime wdt;

        private void buttonTestDate_Click(object sender, EventArgs e)
        {
            wdt = new WurmDateTime(
                Convert.ToInt32(textBoxYear.Text), 
                Convert.ToInt32(textBoxStarfall.Text), 
                Convert.ToInt32(textBoxWeek.Text),
                Convert.ToInt32(textBoxDay.Text), 
                Convert.ToInt32(textBoxHour.Text), 
                Convert.ToInt32(textBoxMinute.Text), 
                Convert.ToInt32(textBoxSecond.Text));
            textBoxWDTOut.Text = wdt.ToString();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            wdt += new TimeSpan(0, 12, 44);

            textBoxWDTOut.Text = wdt.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            wdt = new WurmDateTime(1, WurmStarfall.Saw, 3, WurmDay.Sleep, 5, 12, 55);
            textBoxWDTOut.Text = wdt.ToString();
        }
    }
}
