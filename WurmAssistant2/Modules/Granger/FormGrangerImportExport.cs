using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger
{
    public partial class FormGrangerImportExport : Form
    {
        private GrangerContext _context;
        public FormGrangerImportExport(GrangerContext context)
        {
            _context = context;
            InitializeComponent();
            comboBoxExportedHerd.Items.AddRange(context.Herds.Select(x => x.ToString()).ToArray());
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            try
            {
                var exporter = new HerdExporter();
                var xml = exporter.CreateXML(_context, comboBoxExportedHerd.Text.Trim());
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    xml.Save(saveFileDialog1.FileName);
                    MessageBox.Show("Export completed");
                }
            }
            catch (GrangerException ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.LogError("problem exporting herd", this, ex);
            }
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    var importer = new HerdImporter();
                    importer.ImportHerd(_context, textBoxImportedHerd.Text, openFileDialog1.FileName);
                    MessageBox.Show("Import completed");
                }
            }
            catch (GrangerException ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.LogError("problem importing herd", this, ex);
            }
        }

        private void textBoxImportedHerd_TextChanged(object sender, EventArgs e)
        {
            if (_context.Herds.Any(x => x.HerdID == textBoxImportedHerd.Text.Trim()))
            {
                labelImportError.Text = "This herd already exists";
            }
            else
            {
                labelImportError.Text = "";
            }
        }
    }
}
