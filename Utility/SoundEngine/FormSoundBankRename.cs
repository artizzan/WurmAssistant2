using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Aldurcraft.Utility.SoundEngine
{
    public partial class FormSoundBankRename : Form
    {
        string sndpath;
        public string newpath;

        public FormSoundBankRename(string sndname)
        {
            InitializeComponent();
            this.sndpath = sndname;
            textBoxRename.Text = Path.GetFileNameWithoutExtension(sndname);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            newpath = SoundBank.SoundsDirectory + @"\" + Path.GetFileNameWithoutExtension(textBoxRename.Text) + Path.GetExtension(sndpath);
            if (File.Exists(newpath))
            {
                MessageBox.Show("Sound with that name already exists in Sound Bank");
                DialogResult = System.Windows.Forms.DialogResult.None;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
             MessageBox.Show(SoundBank.SoundsDirectory + @"\" + Path.GetFileNameWithoutExtension(textBoxRename.Text) + Path.GetExtension(sndpath));
        }
    }
}
