using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.WurmOnline.WurmState;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger
{
    public partial class FormChoosePlayers : Form
    {
        public string[] Result = new string[0];

        public FormChoosePlayers(string[] currentPlayers)
        {
            InitializeComponent();
            string[] allPlayers = WurmClient.WurmPaths.GetAllPlayersNames();
            foreach (var player in allPlayers)
            {
                checkedListBoxPlayers.Items.Add(player, currentPlayers.Contains(player));
            }
            BuildResult();
        }

        void BuildResult()
        {
            List<string> items = new List<string>();
            foreach (var item in checkedListBoxPlayers.CheckedItems)
            {
                items.Add((string)item);
            }
            Result = items.ToArray();
        }

        private void checkedListBoxPlayers_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            BuildResult();
        }
    }
}
