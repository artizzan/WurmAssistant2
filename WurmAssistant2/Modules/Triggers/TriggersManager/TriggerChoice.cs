using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    public partial class TriggerChoice : Form
    {
        public ITrigger Result = null;

        public TriggerChoice()
        {
            InitializeComponent();
            CreateButton("Simple", () => Result = new SimpleTrigger());
            CreateButton("Regex", () => Result = new RegexTrigger());
            CreateButton("Action Queue", () => Result = new ActionQueueTrigger());
        }

        void CreateButton(string text, Func<ITrigger> clickAction)
        {
            var btn = new Button();
            btn.Width = 150;
            btn.Height = 30;
            btn.Text = text;
            btn.Click += (sender, args) =>
            {
                clickAction();
                this.DialogResult = DialogResult.OK;
            };
            flowLayoutPanel1.Controls.Add(btn);
        }
    }
}
