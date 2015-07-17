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
    public partial class FormChangelog : Form
    {
        public FormChangelog(string textToShow)
        {
            InitializeComponent();
            textBox1.Text = textToShow;
        }
    }
}
