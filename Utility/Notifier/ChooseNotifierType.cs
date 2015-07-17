using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility.PopupNotify;
using Aldurcraft.Utility.SoundEngine;

namespace Aldurcraft.Utility.Notifier
{
    public partial class ChooseNotifierType : Form
    {
        public INotifier Result = null;

        public ChooseNotifierType(IEnumerable<INotifier> existingNotifiers)
        {
            InitializeComponent();
            var enumerable = existingNotifiers as INotifier[] ?? existingNotifiers.ToArray();
            
            CreateButton("Sound Notifier", 
                () => Result = new SoundNotifier(string.Empty), 
                enumerable.Any(x => x is ISoundNotifier));
            
            CreateButton("Popup Notifier", 
                () => Result = new PopupNotifier(new PopupMessage() { Duration = 3000}), 
                enumerable.Any(x => x is IPopupNotifier));

            CreateButton("Message Notifier (NYI)",
                () => Result = new MessageNotifier(new MessageSystem.Message()),
                true); //enumerable.Any(x => x is IMessageNotifier));
        }

        void CreateButton(string text, Func<INotifier> clickAction, bool disabled)
        {
            var btn = new Button {Width = 150, Height = 30, Text = text};
            if (disabled) btn.Enabled = false;
            else
            {
                btn.Click += (sender, args) =>
                             {
                                 clickAction();
                                 this.DialogResult = DialogResult.OK;
                             };
            }
            flowLayoutPanel1.Controls.Add(btn);
        }
    }
}
