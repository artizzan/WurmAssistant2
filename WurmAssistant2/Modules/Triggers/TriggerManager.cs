using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Windows.Forms;
using Aldurcraft.WurmOnline.WurmLogsManager;
using System.Text.RegularExpressions;
using Aldurcraft.Utility;
using Aldurcraft.Utility.SoundEngine;
using Microsoft.Runtime.CompilerServices;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    public class TriggerManager
    {
        [DataContract]
        public class NotifierSettings
        {
            [DataMember]
            public bool Muted = false;
            [DataMember][Obsolete]
            public double QueueDefDelay;
            [DataMember][Obsolete]
            public bool QueueSoundEnabled = false;
            [DataMember][Obsolete]
            public string QueueSoundName = null;
            [DataMember]
            private List<ITrigger> _triggers;

            private bool IsMuted()
            {
                return Muted;
            }

            public Func<bool> GetMutedEvaluator()
            {
                return IsMuted;
            }

#if DEBUG
            [Obsolete]
            public byte[] HorseListState;
#endif

            [DataMember]
            public byte[] TriggerListState;

            public IEnumerable<ITrigger> Triggers
            {
                get { return _triggers; }
            }

            public void RemoveTrigger(ITrigger trigger)
            {
                _triggers.Remove(trigger);
            }

            public void AddTrigger(ITrigger trigger)
            {
                _triggers.Add(trigger);
            }
                
            [OnDeserializing]
            private void OnDes(StreamingContext context)
            {
                InitMe();
            }

            [OnDeserialized]
            private void AfterDes(StreamingContext context)
            {
                _triggers.ForEach(x => x.MuteChecker = GetMutedEvaluator());
            }

            public NotifierSettings()
            {
                InitMe();
            }

            private void InitMe()
            {
                _triggers = new List<ITrigger>();
                TriggerListState = new byte[0];
            }
        }

        readonly UcPlayerTriggersController _controlUi;
        readonly ModuleTriggers _parentModule;
        public readonly PersistentObject<NotifierSettings> Settings;
        public readonly string Player;

        readonly FormTriggersConfig _triggersConfigUi;

        // previous processed line
        string _lastline;

        //defQueueSoundPlayer = new SB_SoundPlayer(Path.Combine(ParentModule.ModuleAssetDir, "defQueueSound.ogg"));
        //defQueueSoundPlayer.Load(volumeAdjust: false);

        public TriggerManager(ModuleTriggers parentModule, string player, string moduleDataDir)
        {
            this._parentModule = parentModule;
            Player = player;
            string thisNotifierDataDir = Path.Combine(moduleDataDir, player);
            if (!Directory.Exists(thisNotifierDataDir)) Directory.CreateDirectory(thisNotifierDataDir);

            Settings = new PersistentObject<NotifierSettings>(new NotifierSettings());
            Settings.SetFilePathAndLoad(Path.Combine(thisNotifierDataDir, "settings.xml"));

            //create control for Module UI
            _controlUi = new UcPlayerTriggersController();

            //create this notifier UI
            _triggersConfigUi = new FormTriggersConfig(this);

            UpdateMutedState();
            _controlUi.label1.Text = player;
            _controlUi.buttonMute.Click += ToggleMute;
            _controlUi.buttonConfigure.Click += Configure;
            _controlUi.buttonRemove.Click += Stop;

            WurmLogs.SubscribeToLogFeed(this.Player, OnNewLogEvents);
        }

        public UcPlayerTriggersController GetUIHandle()
        {
            return _controlUi;
        }

        private void ToggleMute(object sender, EventArgs e)
        {
            Settings.Value.Muted = !Settings.Value.Muted;
            Settings.DelayedSave();
            UpdateMutedState();
            _triggersConfigUi.UpdateMutedState();
        }

        private bool Muted
        {
            get { return Settings.Value.Muted || _parentModule.Settings.Value.GlobalMute; }
        }

        public void UpdateMutedState()
        {
            if (Settings.Value.Muted) _controlUi.buttonMute.BackgroundImage = Properties.Resources.SoundDisabledSmall;
            else _controlUi.buttonMute.BackgroundImage = Properties.Resources.SoundEnabledSmall;
        }

        ////////////////// 

        public void Update()
        {
            Settings.Update();
            var dtNow = DateTime.Now;
            foreach (var trigger in Settings.Value.Triggers.ToArray())
            {
                trigger.FixedUpdate(dtNow);
            }
        }

        private void Configure(object sender, EventArgs e)
        {
            //open soundnotify form
            ToggleUi();
        }

        public void Stop(object sender, EventArgs e)
        {
            Settings.Save();
            WurmLogs.UnsubscribeFromLogFeed(this.Player, OnNewLogEvents);
            _parentModule.RemoveManager(this);

            _controlUi.Dispose();
            _triggersConfigUi.Close();
        }

        private void OnNewLogEvents(object sender, NewLogEntriesEventArgs e)
        {
            if (e.Entries.PlayerName == this.Player)
            {
                foreach (var entry in e.Entries.AllEntries)
                {
                    HandleNewLogEvents(entry.Entries, entry.LogType);
                }
            }
        }

        private void ToggleUi()
        {
            _triggersConfigUi.ShowThisDarnWindowDammitEx();
        }

        private void HandleNewLogEvents(IEnumerable<string> newLogEvents, GameLogTypes logType)
        {
            _lastline = "";
            var dtNow = DateTime.Now;
            foreach (string logMessage in newLogEvents)
            {
                if (logMessage != null && _lastline != logMessage && logMessage.Trim() != string.Empty)
                {
                    Logger.LogDebug("log message forwarded", this);
                    foreach (var trigger in Settings.Value.Triggers.ToArray())
                    {
                        if (trigger.CheckLogType(logType))
                        {
                            trigger.Update(logMessage, dtNow);
                        }
                    }
                    _lastline = logMessage;
                }
            }
        }
    }
}
