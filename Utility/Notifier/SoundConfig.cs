using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility.Helpers;
using Aldurcraft.Utility.SoundEngine;

namespace Aldurcraft.Utility.Notifier
{
    public partial class SoundConfig : UserControl, INotifierConfig
    {
        private readonly ISoundNotifier _soundNotifier;

        public UserControl ControlHandle { get { return this; } }

        private static Color _soundTextBoxDefBackColor;

        public event EventHandler Removed;

        public SoundConfig(ISoundNotifier soundNotifier)
        {
            InitializeComponent();
            _soundTextBoxDefBackColor = SoundTextBox.BackColor;
            _soundNotifier = soundNotifier;
            SetSoundTextBoxText(_soundNotifier.SoundName);
        }

        private void ChangeButton_Click(object sender, EventArgs e)
        {
            var sound = SoundBank.ChooseSound();
            if (sound != null)
            {
                _soundNotifier.SoundName = sound;
                SetSoundTextBoxText(_soundNotifier.SoundName);
            }
        }

        void SetSoundTextBoxText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                SoundTextBox.BackColor = Color.PapayaWhip;
                SoundTextBox.Text = "no sound set";
            }
            else
            {
                SoundTextBox.BackColor = _soundTextBoxDefBackColor;
                SoundTextBox.Text = text;
            }

        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            EventExtensions.TriggerEventTsafe(this, EventArgs.Empty, Removed);
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            SoundBank.PlaySound(SoundTextBox.Text);
        }
    }
}
