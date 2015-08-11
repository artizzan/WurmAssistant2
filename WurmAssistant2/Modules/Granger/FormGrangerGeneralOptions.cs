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
    public partial class FormGrangerGeneralOptions : Form
    {
        private readonly PersistentObject<GrangerSettings> _settings; 

        public FormGrangerGeneralOptions(PersistentObject<GrangerSettings> settings)
        {
            _settings = settings;
            InitializeComponent();
            InitGuiValues();
        }

        private void InitGuiValues()
        {
            checkBoxAlwaysUpdateUnlessMultiples.Checked = _settings.Value.DoNotBlockDataUpdateUnlessMultiplesInEntireDb;
            timeSpanInputGroomingTime.Value = _settings.Value.ShowGroomingTime;
            checkBoxUpdateAgeHealthAllEvents.Checked = _settings.Value.UpdateHorseDataFromAnyEventLine;
            checkBoxDisableRowColoring.Checked = _settings.Value.DisableRowColoring;
        }

        private void CommitChanges()
        {
            _settings.Value.DoNotBlockDataUpdateUnlessMultiplesInEntireDb = checkBoxAlwaysUpdateUnlessMultiples.Checked;
            _settings.Value.ShowGroomingTime = timeSpanInputGroomingTime.Value;
            _settings.Value.UpdateHorseDataFromAnyEventLine = checkBoxUpdateAgeHealthAllEvents.Checked;
            _settings.Value.DisableRowColoring = checkBoxDisableRowColoring.Checked;
            _settings.Save();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            CommitChanges();
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
