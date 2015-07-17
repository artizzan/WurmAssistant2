using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HSLColor_tester
{
    using CustomColors;
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            AdjustColor();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            AdjustColor();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            AdjustColor();
        }

        void AdjustColor()
        {
            HSLColor color = new HSLColor((double)trackBar1.Value, (double)trackBar2.Value, (double)trackBar3.Value);
            textBoxColor.Text = color.ToString();
            listView1.Items[1].BackColor = (Color)color;
        }
    }
}
