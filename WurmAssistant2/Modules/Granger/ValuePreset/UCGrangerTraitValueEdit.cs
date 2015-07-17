using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger
{
    public partial class UCGrangerTraitValueEditBox : UserControl
    {
        private HorseTrait _Trait;

        public HorseTrait Trait { get { return _Trait; } set { _Trait = value; textBox1.Text = value.ToString(); } }
        public int Value { get { return (int)numericUpDown1.Value; } set { numericUpDown1.Value = GeneralHelper.ConstrainValue<int>(value, -1000, 1000); } }
        public bool ReadOnly { set { numericUpDown1.Enabled = !value; } }

        public UCGrangerTraitValueEditBox()
        {
            InitializeComponent();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (TraitValueChanged != null) TraitValueChanged(this, EventArgs.Empty);
        }

        public event EventHandler TraitValueChanged;
    }
}
