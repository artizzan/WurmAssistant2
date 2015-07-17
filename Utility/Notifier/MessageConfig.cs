using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility.Helpers;
using Aldurcraft.Utility.MessageSystem;

namespace Aldurcraft.Utility.Notifier
{
    public partial class MessageConfig : UserControl, INotifierConfig
    {
        private readonly IMessageNotifier _messageNotifier;
        private readonly bool _initComplete;

        public event EventHandler Removed;

        public UserControl ControlHandle { get { return this; }}

        public MessageConfig(IMessageNotifier messageNotifier)
        {
            InitializeComponent();
            _messageNotifier = messageNotifier;
            ContentTextBox.Text = messageNotifier.Content;
            TitleTextBox.Text = messageNotifier.Title;

            _initComplete = true;
        }

        private void Save()
        {
            if (_initComplete)
            {
                _messageNotifier.Content = ContentTextBox.Text;
                _messageNotifier.Title = TitleTextBox.Text;
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            EventExtensions.TriggerEventTsafe(this, EventArgs.Empty, Removed);
        }

        private void TitleTextBox_TextChanged(object sender, EventArgs e)
        {
            Save();
        }

        private void ContentTextBox_TextChanged(object sender, EventArgs e)
        {
            Save();
        }
    }
}
