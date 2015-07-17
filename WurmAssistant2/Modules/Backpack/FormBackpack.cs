using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Backpack
{
    public partial class FormBackpack : Form
    {
        private ModuleBackpack ParentModule;
        bool _initialized = false;

        public FormBackpack(ModuleBackpack moduleBackpack)
        {
            InitializeComponent();
            this.ParentModule = moduleBackpack;

            _initialized = true;
        }

        private void FormBackpack_Load(object sender, EventArgs e)
        {
            //toolbelt extractor
            comboBoxToolbeltsPlayer.Items.AddRange(WurmState.WurmClient.WurmPaths.GetAllPlayersNames());

            //now do the silly winforms window visibility workarounds
            timer1.Enabled = true;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("http://wurmassistant.wikia.com/wiki/Backpack");
            }
            catch (Exception _e)
            {
                Logger.LogError("problem opening link", this, _e);
            }
        }

        private void buttonMakeToolbeltExec_Click(object sender, EventArgs e)
        {
            if (comboBoxToolbeltsPlayer.SelectedItem == null) return;

            //get toolbelt info
            string execfile = string.Empty;
            string toolbeltFilePath = WurmState.WurmClient.WurmPaths.GetFilePathForPlayerData(comboBoxToolbeltsPlayer.SelectedItem.ToString());

            try
            {
                string fileContents = File.ReadAllText(toolbeltFilePath);

                //parse into exec file
                MatchCollection matches = Regex.Matches(fileContents, @"\w+\d+=\d+");
                foreach (Match item in matches)
                {
                    Match match = Regex.Match(item.Value, @"\w+(\d+)=(\d+)");
                    execfile += string.Format("settoolbelt {0} {1}",
                        match.Groups[2], match.Groups[1]);
                    execfile += "\r\n";
                }

                //display dialog to save the file
                SaveFileDialog dialog = new SaveFileDialog();

                string path = WurmState.WurmClient.WurmPaths.WurmDir;
                if (path != null)
                {
                    dialog.InitialDirectory = path;
                }

                dialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;
                dialog.FileName = "myToolbeltSetup.txt";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    using (Stream stream = dialog.OpenFile())
                    {
                        if (stream != null)
                        {
                            using (StreamWriter sr = new StreamWriter(stream))
                            {
                                sr.Write(execfile);
                            }
                        }
                        else
                        {
                            ErrorDispl("problem saving file, stream == null");
                        }
                    }
                }
            }
            catch (Exception _e)
            {
                ErrorDispl("opening playerdata.txt failed", _e);
            }
        }

        private void ErrorDispl(string text, Exception _e = null)
        {
            MessageBox.Show("error: " + text);
            Logger.LogError(text, _e);
        }

        private void FormBackpack_Resize(object sender, EventArgs e)
        {
            if (_initialized)
            {
                ParentModule.Settings.Value.SavedWindowSize = new Point(this.Size.Width, this.Size.Height);
                ParentModule.Settings.DelayedSave();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Size = new Size(ParentModule.Settings.Value.SavedWindowSize);
            timer1.Enabled = false;
        }
    }
}
