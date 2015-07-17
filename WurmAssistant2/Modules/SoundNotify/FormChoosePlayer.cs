using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.SoundNotify
{
    public partial class FormChoosePlayer : Form
    {
        public FormChoosePlayer(string[] availableChoices)
        {
            InitializeComponent();
            checkedListBox1.Items.AddRange(availableChoices);
        }

        private void FormChoosePlayer_Load(object sender, EventArgs e)
        {

        }

        public string[] result = new string[0];

        private void button1_Click(object sender, EventArgs e)
        {
            if (checkedListBox1.CheckedItems.Count > 0)
            {
                List<string> list = new List<string>();
                foreach (var item in checkedListBox1.CheckedItems)
                {
                    list.Add(item.ToString());
                }
                result = list.ToArray();
            }
        }
    }
}
