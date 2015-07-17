using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Aldurcraft.Utility.MessageSystem;
using Aldurcraft.Utility.PopupNotify;
using Aldurcraft.Utility.SoundEngine;
using Aldurcraft.Utility;

namespace Aldurcraft.Utility.Notifier
{
    [DataContract]
    public class SoundNotifier : NotifierBase, ISoundNotifier
    {
        [DataMember] 
        private string _soundName;
        public string SoundName
        {
            get { return _soundName; }
            set
            {
                if (_soundName == null) value = string.Empty;
                _soundName = value;
                SoundPlayer = SoundBank.GetSoundPlayerNoNulls(value);
            }
        }

        private SB_SoundPlayer _soundPlayer;
        private SB_SoundPlayer SoundPlayer
        {
            get { return _soundPlayer; }
            set
            {
                _soundPlayer = value;
            }
        }

        public override bool HasEmptySound
        {
            get
            {
                if (SoundPlayer == null) return true;
                return string.IsNullOrEmpty(SoundPlayer.SoundName);
            }
        }

        public SoundNotifier(string soundName)
        {
            _soundName = soundName;
        }

        public override INotifierConfig GetConfig()
        {
            return new SoundConfig(this);
        }

        public override void Notify()
        {
            var played = SoundPlayer.Play();
            if (!played) this.SoundName = string.Empty;
        }

        [OnDeserializing]
        private void OnDes(StreamingContext context)
        {
            // do not use prop because it would try setting sound player
            _soundName = string.Empty;
        }

        [OnDeserialized]
        private void AfterDes(StreamingContext context)
        {
            // get proper sound player for deserialized sound name
            SoundName = _soundName;
        }

        private void Init()
        {
            SoundPlayer = SoundBank.GetSoundPlayerNoNulls(_soundName);
        }
    }
}
