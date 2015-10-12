using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger
{
    public partial class FormHerdMerge : Form
    {
        private GrangerContext Context;
        private FormGrangerMain MainForm;
        private string SourceHerdName;

        public FormHerdMerge(GrangerContext context, FormGrangerMain mainForm, string sourceHerdName)
        {
            this.Context = context;
            this.MainForm = mainForm;
            this.SourceHerdName = sourceHerdName;
            InitializeComponent();
            textBoxFromHerd.Text = SourceHerdName;
            comboBoxToHerd.Items.AddRange(Context.Herds.Where(x => x.HerdID != sourceHerdName).ToArray());
            listBoxFromHerd.Items.AddRange(Context.Horses.Where(x => x.Herd == SourceHerdName).ToArray());
        }

        private void comboBoxToHerd_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxToHerd.SelectedIndex > -1)
            {
                listBoxToHerd.Items.Clear();
                listBoxToHerd.Items.AddRange(
                    Context.Horses.Where(x => x.Herd == comboBoxToHerd.SelectedItem.ToString()).ToArray());
                buttonOK.Enabled = true;
            }
            else buttonOK.Enabled = false;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                Context.MergeHerds(textBoxFromHerd.Text, comboBoxToHerd.Text);
            }
            catch (Exception _e)
            {
                MessageBox.Show("there was a problem with merging herds:\r\n" + _e.Message);
                if (_e is GrangerContext.DuplicateHorseIdentityException) Logger.LogDiag("merging herds failed due non-unique creatures", this, _e);
                else Logger.LogError("merge herd problem", this, _e);
            }
        }

        private void listBoxFromHerd_DoubleClick(object sender, EventArgs e)
        {
            //TODO show horse info
        }

        private void listBoxToHerd_DoubleClick(object sender, EventArgs e)
        {
            //TODO show horse info
        }
    }
}
