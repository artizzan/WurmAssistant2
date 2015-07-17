using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Calendar
{
    public partial class FormChooseSeasons : Form
    {
        public FormChooseSeasons(string[] items, string[] tracked)
        {
            InitializeComponent();
            int indexcount = 0;

            foreach (string item in items)
            {
                checkedListBox1.Items.Add(item);
                checkedListBox1.SetItemChecked(indexcount, tracked.Contains(item, StringComparer.InvariantCultureIgnoreCase));
                indexcount++;
            }
        }
    }
}
