using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Ex;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms;
using Aldurcraft.Utility;
using Aldurcraft.Utility.Notifier;
using Aldurcraft.Utility.PopupNotify;
using Aldurcraft.WurmOnline.WurmAssistant2.Properties;
using Message = Aldurcraft.Utility.MessageSystem.Message;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    public partial class EditTrigger : Form
    {
        private readonly ITrigger _trigger;

        public EditTrigger(ITrigger trigger)
        {
            _trigger = trigger;
            InitializeComponent();
            Text = trigger.TypeAspect.CapitalizeEx() + " Trigger";
            trigger.Configs.ToList()
                .ForEach(x => SettingsLayout.Controls.Add(x.ControlHandle));
            trigger.GetNotifiers().ToList()
                .ForEach(AddConfigurator);
        }

        private void AddNotificationButton_Click(object sender, EventArgs e)
        {
            IEnumerable<INotifier> restrictNotifiers = _trigger.GetNotifiers();
            var ui = new ChooseNotifierType(restrictNotifiers);
            if (ui.ShowDialogCenteredEx(this) == DialogResult.OK)
            {
                _trigger.AddNotifier(ui.Result);
                AddConfigurator(ui.Result);
            }
        }

        void AddConfigurator(INotifier notifier)
        {
            var configurator = notifier.GetConfig();
            var uc = configurator.ControlHandle;
            NotificationsLayout.Controls.Add(uc);
            configurator.Removed += (o, args) =>
            {
                NotificationsLayout.Controls.Remove(configurator.ControlHandle);
                _trigger.RemoveNotifier(notifier);
            };
        }

        private void SettingsLayout_Layout(object sender, LayoutEventArgs e)
        {
            foreach (UserControl ctrl in SettingsLayout.Controls)
            {
                ctrl.Width = SettingsLayout.Width - 25;
            }
        }
    }
}
