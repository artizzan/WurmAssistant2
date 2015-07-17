using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WA2_Test
{
    using Aldurcraft.WurmOnline.WurmState;

    public partial class WurmClientState_Test : Form
    {
        public WurmClientState_Test()
        {
            InitializeComponent();
            textBoxInitResult.Text = WurmClient.InitSuccessful.ToString();
            listBoxPlayers.Items.AddRange(WurmClient.WurmPaths.GetAllPlayersNames());
            listBoxChangeLogging.Items.AddRange(Enum.GetNames(typeof(WurmClient.Configs.EnumLoggingType)));
            listBoxChangeSkillgain.Items.AddRange(Enum.GetNames(typeof(WurmClient.Configs.EnumSkillGainRate)));
        }

        private void buttonGetIintState_Click(object sender, EventArgs e)
        {
            textBoxInitResult.Text = WurmClient.InitSuccessful.ToString();
        }

        private void buttonApplyWurmDirOverride_Click(object sender, EventArgs e)
        {
            WurmClient.OverrideWurmDir(textBoxOverrideWurmDir.Text);
        }

        private void buttonReinit_Click(object sender, EventArgs e)
        {
            textBoxInitResult.Text = WurmClient.Reinitialize().ToString();
        }

        WurmClient.Configs.ConfigData currentconfig;

        private void listBoxPlayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxPlayers.SelectedIndex >= 0)
            {
                string player = listBoxPlayers.SelectedItem.ToString();
                currentconfig = WurmClient.PlayerConfigurations.GetThisPlayerConfig(player);
                textBoxConfForPlayer.Text = player;
                List<string> settings = new List<string>();
                if (currentconfig != null)
                {
                    settings.Add("config name: " + currentconfig.ConfigName.ToString()); //excetion caused here
                    settings.Add("---");
                    settings.Add("AutoRunSource: " + currentconfig.AutoRunSource.ToString());
                    settings.Add("CustomTimerSource: " + currentconfig.CustomTimerSource.ToString());
                    settings.Add("EventLoggingType: " + currentconfig.EventLoggingType.ToString());
                    settings.Add("ExecSource: " + currentconfig.ExecSource.ToString());
                    settings.Add("IrcLoggingType: " + currentconfig.IrcLoggingType.ToString());
                    settings.Add("KeyBindSource: " + currentconfig.KeyBindSource.ToString());
                    settings.Add("NoSkillGainOnAlignment: " + currentconfig.NoSkillMessageOnAlignmentChange.ToString());
                    settings.Add("NoSkillGainOnFavor: " + currentconfig.NoSkillMessageOnFavorChange.ToString());
                    settings.Add("OtherLoggingType: " + currentconfig.OtherLoggingType.ToString());
                    settings.Add("SaveSkillsOnQuit: " + currentconfig.SaveSkillsOnQuit.ToString());
                    settings.Add("SkillGainRate: " + currentconfig.SkillGainRate.ToString());
                    settings.Add("TimestampMessages: " + currentconfig.TimestampMessages.ToString());
                }
                else
                {
                    settings.Add("no config available");
                }
                textBoxSettings.Lines = settings.ToArray();
            }
        }

        private void listBoxChangeLogging_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxChangeLogging.SelectedIndex >= 0)
            {
                if (currentconfig != null)
                    currentconfig.SetCommonLoggingMode((WurmClient.Configs.EnumLoggingType)Enum.Parse(typeof(WurmClient.Configs.EnumLoggingType), listBoxChangeLogging.SelectedItem.ToString()));
                listBoxPlayers_SelectedIndexChanged(null, null);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (currentconfig != null)
                currentconfig.SetNoSkillMessageOnAlignmentChange(false);
            listBoxPlayers_SelectedIndexChanged(null, null);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (currentconfig != null)
                currentconfig.SetNoSkillMessageOnAlignmentChange(true);
            listBoxPlayers_SelectedIndexChanged(null, null);
        }

        private void listBoxChangeSkillgain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxChangeSkillgain.SelectedIndex >= 0)
            {
                if (currentconfig != null)
                    currentconfig.SetSkillGainRate((WurmClient.Configs.EnumSkillGainRate)Enum.Parse(typeof(WurmClient.Configs.EnumSkillGainRate), listBoxChangeSkillgain.SelectedItem.ToString()));
                listBoxPlayers_SelectedIndexChanged(null, null);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (currentconfig != null)
                currentconfig.SetNoSkillMessageOnFavorChange(false);
            listBoxPlayers_SelectedIndexChanged(null, null);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (currentconfig != null)
                currentconfig.SetNoSkillMessageOnFavorChange(true);
            listBoxPlayers_SelectedIndexChanged(null, null);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (currentconfig != null)
                currentconfig.SetSaveSkillsOnQuit(false);
            listBoxPlayers_SelectedIndexChanged(null, null);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (currentconfig != null)
                currentconfig.SetSaveSkillsOnQuit(true);
            listBoxPlayers_SelectedIndexChanged(null, null);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (currentconfig != null)
                currentconfig.SetTimestampMessages(false);
            listBoxPlayers_SelectedIndexChanged(null, null);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (currentconfig != null)
                currentconfig.SetTimestampMessages(true);
            listBoxPlayers_SelectedIndexChanged(null, null);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //stresstest
            alternateStressTest = false;
            backgroundWorker1.RunWorkerAsync();
            backgroundWorker2.RunWorkerAsync();
            backgroundWorker3.RunWorkerAsync();
            backgroundWorker4.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!alternateStressTest)
                for (int i = 0; i < 10; i++)
                    Console.WriteLine("Monthly: " + currentconfig.SetCommonLoggingMode(WurmClient.Configs.EnumLoggingType.Monthly));
            else
                for (int i = 0; i < 10; i++)
                    Console.WriteLine("Never: " + currentconfig.SetSkillGainRate(WurmClient.Configs.EnumSkillGainRate.Never));
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!alternateStressTest)
                for (int i = 0; i < 10; i++)
                    Console.WriteLine("Daily: " + currentconfig.SetCommonLoggingMode(WurmClient.Configs.EnumLoggingType.Daily));
            else
                for (int i = 0; i < 10; i++)
                    Console.WriteLine("per0_001: " + currentconfig.SetSkillGainRate(WurmClient.Configs.EnumSkillGainRate.per0_001));
        }

        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!alternateStressTest)
                for (int i = 0; i < 10; i++)
                    Console.WriteLine("Never: " + currentconfig.SetCommonLoggingMode(WurmClient.Configs.EnumLoggingType.Never));
            else
                for (int i = 0; i < 10; i++)
                    Console.WriteLine("Per0_01: " + currentconfig.SetSkillGainRate(WurmClient.Configs.EnumSkillGainRate.Per0_01));
        }

        private void backgroundWorker4_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!alternateStressTest)
                for (int i = 0; i < 10; i++)
                    Console.WriteLine("OneFile: " + currentconfig.SetCommonLoggingMode(WurmClient.Configs.EnumLoggingType.OneFile));
            else
                for (int i = 0; i < 10; i++)
                    Console.WriteLine("Per0_1: " + currentconfig.SetSkillGainRate(WurmClient.Configs.EnumSkillGainRate.Per0_1));
        }

        bool alternateStressTest = false;

        private void button10_Click(object sender, EventArgs e)
        {
            alternateStressTest = true;
            backgroundWorker1.RunWorkerAsync();
            backgroundWorker2.RunWorkerAsync();
            backgroundWorker3.RunWorkerAsync();
            backgroundWorker4.RunWorkerAsync();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            listBoxPlayers_SelectedIndexChanged(null, null);
        }
    }
}
