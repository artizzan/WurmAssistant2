using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using Aldurcraft.WurmOnline.WurmLogsManager;
using Aldurcraft.WurmOnline.WurmLogsManager.Searcher;
using Aldurcraft.Utility;

namespace WA2_Test
{
    public partial class Searcher_Test : Form
    {
        public Searcher_Test()
        {
            InitializeComponent();
        }

        private void Searcher_Test_Load(object sender, EventArgs e)
        {
            Logger.SetTBOutput(textBox1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WurmLogSearcherAPI.ToggleUI();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GetSearchResults();
        }

        async Task GetSearchResults()
        {
            LogSearchData searchdata = new LogSearchData();
            searchdata.SetSearchCriteria(
                "Aldur",
                GameLogTypes.Alliance,
                DateTime.Now - TimeSpan.FromDays(2),
                DateTime.Now,
                "",
                SearchTypes.RegexEscapedCaseIns);
            LogSearchData results = await WurmLogSearcherAPI.SearchWurmLogsAsync(searchdata);
            textBox2.Lines = results.AllLines.ToArray();
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            Dictionary<Aldurcraft.WurmOnline.WurmState.WurmServer.ServerInfo.ServerGroup, float> result = 
                await WurmLogSearcherAPI.GetSkillsForPlayerAsync(
                "Aldur", 30, "Repairing");

            try
            {
                foreach (var keyval in result)
                {
                    textBox2.Text += keyval.Key.ToString() + ": " + keyval.Value.ToString();
                }
            }
            catch
            {
                textBox2.Text = "Exception";
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            WurmLogSearcherAPI.Initialize();
        }
    }
}
