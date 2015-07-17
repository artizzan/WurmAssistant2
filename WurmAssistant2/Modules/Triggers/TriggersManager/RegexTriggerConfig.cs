using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    public partial class RegexTriggerConfig : UserControl, ITriggerConfig
    {
        private readonly RegexTrigger _regexTrigger;

        private bool _initComplete = false;
        public RegexTriggerConfig(RegexTrigger regexTrigger)
        {
            _regexTrigger = regexTrigger;
            InitializeComponent();
            _initComplete = true;
        }

        public UserControl ControlHandle { get { return this; } }

        private void WhatsRegularExprLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Wa2SpellBook.ExecCatchLog(() => Process.Start(@"http://searchsoftwarequality.techtarget.com/definition/regular-expression"), this);
        }

        private void RegexGuideLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Wa2SpellBook.ExecCatchLog(() => Process.Start(@"http://www.codeproject.com/Articles/9099/The-30-Minute-Regex-Tutorial"), this);
        }
    }
}
