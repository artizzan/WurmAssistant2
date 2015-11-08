using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility.SoundEngine;
using AldursLab.WurmAssistantDataTransfer;
using WurmAssistantDataTransfer.Dtos;

namespace Aldurcraft.WurmOnline.WurmAssistant2.Engine
{
    public partial class DataExportForm : Form
    {
        public DataExportForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Export();
        }

        public void Export()
        {
            try
            {
                WurmAssistantDto dto = new WurmAssistantDto()
                {
                    DataSourceEnum = DataSource.WurmAssistant2,
                    Version = 0
                };

                // including all sounds..
                var allSoundNames = SoundBank.GetSoundsArray();
                foreach (var soundName in allSoundNames)
                {
                    dto.TryMergeSoundAndGet(soundName);
                }

                // exporting data from each module, if anything to export
                foreach (var assistantModule in ModuleManager.GetActiveModules())
                {
                    assistantModule.PopulateDataTransfer(dto);
                }

                // saving the file
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(saveFileDialog1.FileName))
                    {
                        File.Delete(saveFileDialog1.FileName);
                    }
                    var dataTransferManager = new DataTransferManager();
                    dataTransferManager.SaveToFile(saveFileDialog1.FileName, dto);
                    Process.Start(Path.GetDirectoryName(saveFileDialog1.FileName));
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
