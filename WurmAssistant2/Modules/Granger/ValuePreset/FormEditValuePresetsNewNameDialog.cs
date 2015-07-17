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
    public partial class FormEditValuePresetsNewNameDialog : Form
    {
        private FormEditValuePresets FormEditValuePresets;
        private GrangerContext Context;

        public string Result { get; private set; }

        HashSet<string> TakenValueMapIDs = new HashSet<string>();

        public FormEditValuePresetsNewNameDialog(FormEditValuePresets formEditValuePresets, GrangerContext context)
        {
            this.FormEditValuePresets = formEditValuePresets;
            this.Context = context;

            InitializeComponent();

            var uniqueValMapIDs = Context.TraitValues.AsEnumerable().Select(x => x.ValueMapID).Distinct();
            foreach (var mapID in uniqueValMapIDs)
            {
                TakenValueMapIDs.Add(mapID);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim().Length == 0)
            {
                buttonOK.Enabled = false;
            }
            else if (TakenValueMapIDs.Contains(textBox1.Text.Trim()))
            {
                buttonOK.Enabled = false;
                labelWarn.Visible = true;
            }
            else
            {
                buttonOK.Enabled = true;
                labelWarn.Visible = false;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Result = textBox1.Text.Trim();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (buttonOK.Enabled) buttonOK.PerformClick();
                e.SuppressKeyPress = true;
            }
        }

        private void FormEditValuePresetsNewNameDialog_Load(object sender, EventArgs e)
        {
            this.Location = FormHelper.GetCenteredChildPositionRelativeToParentWorkAreaBound(this, FormEditValuePresets);
        }
    }
}
