using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility.Helpers;

namespace Aldurcraft.Utility.Notifier
{
    public partial class PopupConfig : UserControl, INotifierConfig
    {
        private readonly IPopupNotifier _popupNotifier;
        private readonly bool _initComplete;

        public event EventHandler Removed;

        public UserControl ControlHandle { get { return this; } }

        public PopupConfig(IPopupNotifier popupNotifier)
        {
            InitializeComponent();
            _popupNotifier = popupNotifier;
            ContentTextBox.Text = popupNotifier.Content;
            TitleTextBox.Text = popupNotifier.Title;
            DurationNumeric.Value = (decimal)popupNotifier.Duration.TotalSeconds;
            StayUntilClickedCheckBox.Checked = popupNotifier.StayUntilClicked;
            DurationNumeric.Enabled = !popupNotifier.StayUntilClicked;

            _initComplete = true;
        }

        private void Save()
        {
            if (_initComplete)
            {
                _popupNotifier.Content = ContentTextBox.Text;
                _popupNotifier.Title = TitleTextBox.Text;
                _popupNotifier.Duration = TimeSpan.FromSeconds((double) DurationNumeric.Value);
                _popupNotifier.StayUntilClicked = StayUntilClickedCheckBox.Checked;
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

        private void DurationNumeric_ValueChanged(object sender, EventArgs e)
        {
            Save();
        }

        private void StayUntilClickedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Save();
            DurationNumeric.Enabled = !StayUntilClickedCheckBox.Checked;
        }
    }
}
