using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Timers
{
    public partial class FormTimerGlobalSettings : Form
    {
        private readonly FormTimers _formTimers;

        public FormTimerGlobalSettings(FormTimers formTimers)
        {
            _formTimers = formTimers;
            InitializeComponent();

            checkBoxWidgetView.Checked = _formTimers.WidgetModeEnabled;
            textBoxWidgetSample.BackColor = _formTimers.WidgetBgColor;
            textBoxWidgetSample.ForeColor = _formTimers.WidgetForeColor;
        }

        private void checkBoxWidgetView_CheckedChanged(object sender, EventArgs e)
        {
            _formTimers.WidgetModeEnabled = checkBoxWidgetView.Checked;
        }

        private void buttonChangeWidgetBgColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = _formTimers.WidgetBgColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                _formTimers.WidgetBgColor = colorDialog1.Color;
                textBoxWidgetSample.BackColor = colorDialog1.Color;
            }
        }

        private void textBoxWidgetSample_TextChanged(object sender, EventArgs e)
        {

        }

        private void buttonSetWidgetFontColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = _formTimers.WidgetForeColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                _formTimers.WidgetForeColor = colorDialog1.Color;
                textBoxWidgetSample.ForeColor = colorDialog1.Color;
            }
        }

        private void buttonResetWidgetDefaultColor_Click(object sender, EventArgs e)
        {
            textBoxWidgetSample.BackColor = _formTimers.WidgetBgColor = DefaultBackColor;
            textBoxWidgetSample.ForeColor = _formTimers.WidgetForeColor = DefaultForeColor;
        }
    }
}
