using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Aldurcraft.Utility;
using System.IO;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Timers
{
    public class ModuleTimers : AssistantModule
    {
        [DataContract]
        public class TimersSettings
        {
            [DataMember]
            public HashSet<string> ActivePlayers = new HashSet<string>();

            [DataMember]
            public System.Drawing.Point SavedWindowSize = new System.Drawing.Point();

            [DataMember]
            public bool WidgetModeEnabled;

            [DataMember]
            public bool WidgetModeClickThroughEnabled; //not implemented

            [DataMember]
            public Color WidgetBgColor;

            [DataMember]
            public Color WidgetForeColor;

            public TimersSettings()
            {
                InitMe();
            }

            [OnDeserializing]
            void OnDes(StreamingContext context)
            {
                InitMe();
            }

            void InitMe()
            {
                ActivePlayers = new HashSet<string>();
                WidgetBgColor = SystemColors.Control;
                WidgetForeColor = SystemColors.ControlText;
            }
        }

        List<PlayerTimersGroup> TimerGroups = new List<PlayerTimersGroup>();

        public PersistentObject<TimersSettings> Settings;
        FormTimers ModuleUI;

        public override void Initialize()
        {
            base.Initialize();
            Settings = new PersistentObject<TimersSettings>(new TimersSettings());
            Settings.FilePath = Path.Combine(this.ModuleDataDir, "settings.xml");
            if (!Settings.Load())
            {
                Settings.Save();
            }
            WurmTimerDescriptors.RemovedCustomTimer += WurmTimerDescriptors_RemovedCustomTimer;
            ModuleUI = new FormTimers(this);
            WurmTimerDescriptors.LoadCustomTimers(Path.Combine(this.ModuleDataDir, "customTimers.xml"));
            foreach (string player in Settings.Value.ActivePlayers)
            {
                AddNewPlayerGroup(player);
            }
        }

        void WurmTimerDescriptors_RemovedCustomTimer(object sender, WurmTimerDescriptors.RemovedTimerEventArgs e)
        {
            foreach (var timergroup in TimerGroups)
            {
                timergroup.RemoveDeletedCustomTimer(e.NameID);
            }
        }

        public override void OpenUI(object sender, EventArgs e)
        {
            ModuleUI.ShowThisDarnWindowDammitEx();
        }

        public override void Update(bool engineSleeping)
        {
            Settings.Update();
            foreach (var timergroup in TimerGroups)
            {
                timergroup.Update(engineSleeping);
            }
        }

        public override void Stop()
        {
            Settings.Save();
            foreach (var timergroup in TimerGroups)
            {
                timergroup.Stop();
            }
            ModuleUI.Dispose();
        }

        internal void RegisterTimersGroup(UControlPlayerLayout layoutControl)
        {
            ModuleUI.RegisterTimersGroup(layoutControl);
        }

        internal void UnregisterTimersGroup(UControlPlayerLayout layoutControl)
        {
            ModuleUI.UnregisterTimersGroup(layoutControl);
        }

        internal string[] GetActivePlayerGroups()
        {
            var result = new List<string>();
            foreach (var name in Settings.Value.ActivePlayers)
            {
                result.Add(name);
            }
            return result.ToArray();
        }

        internal void AddNewPlayerGroup(string player)
        {
            TimerGroups.Add(new PlayerTimersGroup(this, player));
            Settings.Value.ActivePlayers.Add(player);
            Settings.DelayedSave();
        }

        internal void RemovePlayerGroup(string player)
        {
            var group = TimerGroups.Where(x => x.Player == player).First();
            group.Stop();
            TimerGroups.Remove(group);
            Settings.Value.ActivePlayers.Remove(player);
            Settings.DelayedSave();
        }

        public void SaveSettings()
        {
            Settings.DelayedSave();
        }

        internal FormTimers GetModuleUI()
        {
            return this.ModuleUI;
        }
    }
}
