using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger
{
    public partial class FormGrangerNewInfo : Form
    {
        private Aldurcraft.Utility.PersistentObject<GrangerSettings> Settings;

        public FormGrangerNewInfo(Aldurcraft.Utility.PersistentObject<GrangerSettings> Settings)
        {
            this.Settings = Settings;
            InitializeComponent();
        }

        private void FormGrangerNewInfo_Load(object sender, EventArgs e)
        {
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Value.DoNotShowReadFirstWindow = checkBox1.Checked;
            Settings.DelayedSave();
        }
    }
}
