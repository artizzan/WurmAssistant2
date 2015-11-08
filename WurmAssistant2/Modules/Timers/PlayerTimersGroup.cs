using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using Aldurcraft.Utility;
using Aldurcraft.WurmOnline.WurmLogsManager;
using Aldurcraft.WurmOnline.WurmLogsManager.Searcher;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Timers
{
    using ServerInfo = Aldurcraft.WurmOnline.WurmState.WurmServer.ServerInfo;

    public class PlayerTimersGroup
    {
        [DataContract]
        public class GroupSettings
        {
            [DataMember]
            public HashSet<WurmTimerDescriptors.TimerType> ActiveTimers = new HashSet<WurmTimerDescriptors.TimerType>();
            [DataMember] //saved to figure, on which servers is current character, in each group
            public Dictionary<ServerInfo.ServerGroup, string> GroupToServerMap = new Dictionary<ServerInfo.ServerGroup, string>();
            [DataMember] //saved to remember last group this char was on
            public ServerInfo.ServerGroup _currentServerGroup = ServerInfo.ServerGroup.Unknown;
            [DataMember] //saved to make init searches quicker
            public DateTime LastServerGroupCheckup = DateTime.MinValue;
            [DataMember] //last server this char was on
            public string CurrentServerName = null;

            [OnDeserialized]
            void FixMe(StreamingContext context)
            {
                if (ActiveTimers == null) ActiveTimers = new HashSet<WurmTimerDescriptors.TimerType>();
                if (GroupToServerMap == null) GroupToServerMap = new Dictionary<ServerInfo.ServerGroup, string>();
            }
        }

        public List<WurmTimer> WurmTimers = new List<WurmTimer>();

        public string Player { get; private set; }

        UControlPlayerLayout LayoutControl;
        ModuleTimers ParentModule;

        bool currentServerGroupFound = false;

        public PersistentObject<GroupSettings> Settings;
        public string ThisGroupDir { get; private set; }

        public ServerInfo.ServerGroup CurrentServerGroup
        {
            get { return Settings.Value._currentServerGroup; }
            private set { Settings.Value._currentServerGroup = value; Settings.DelayedSave(); }
        }

        public string CurrentServerName { get { return Settings.Value.CurrentServerName; } }

        public PersistentObject<ModuleTimers.TimersSettings> GlobalSettings { get { return ParentModule.Settings; } }

        public PlayerTimersGroup(ModuleTimers parentModule, string player)
        {
            this.ParentModule = parentModule;
            if (player != null) this.Player = player;

            Settings = new PersistentObject<GroupSettings>(new GroupSettings());
            ThisGroupDir = Path.Combine(ParentModule.ModuleDataDir, Player);
            Settings.FilePath = Path.Combine(ThisGroupDir, "activeTimers.xml");
            if (!Settings.Load()) Settings.Save();

            LayoutControl = new UControlPlayerLayout(this);
            ParentModule.RegisterTimersGroup(LayoutControl);
            WurmLogs.SubscribeToLogFeed(Player, OnNewLogEvents);
            // init timers AT THE END of this async method
            // also may need to block adding/removing timers until this is done
            PerformAsyncInits(Settings.Value.LastServerGroupCheckup);
        }

        private async Task PerformAsyncInits(DateTime lastCheckup)
        {
            try
            {
                TimeSpan timeToCheck = DateTime.Now - lastCheckup;
                if (timeToCheck > TimeSpan.FromDays(120)) timeToCheck = TimeSpan.FromDays(120);
                if (timeToCheck < TimeSpan.FromDays(7)) timeToCheck = TimeSpan.FromDays(7);

                LogSearchData lgs = new LogSearchData();
                lgs.SetSearchCriteria(
                    Player,
                    GameLogTypes.Event,
                    DateTime.Now - timeToCheck,
                    DateTime.Now,
                    "",
                    SearchTypes.RegexEscapedCaseIns);

                lgs = await WurmLogSearcherAPI.SearchWurmLogsAsync(lgs);

                ServerInfo.ServerGroup mostRecentGroup = ServerInfo.ServerGroup.Unknown;
                string mostRecentServerName = null;

                foreach (string line in lgs.AllLines)
                {
                    if (line.Contains("You are on"))
                    {
                        string serverName;
                        ServerInfo.ServerGroup group = WurmLogSearcherAPI.GetServerGroupFromLine(line, out serverName);
                        if (group != ServerInfo.ServerGroup.Unknown)
                        {
                            if (!String.IsNullOrEmpty(serverName)) Settings.Value.GroupToServerMap[group] = serverName;
                            mostRecentServerName = serverName;
                            mostRecentGroup = group;
                        }
                    }
                }

                if (mostRecentGroup != ServerInfo.ServerGroup.Unknown && !currentServerGroupFound)
                {
                    CurrentServerGroup = mostRecentGroup;
                    if (mostRecentServerName != null) Settings.Value.CurrentServerName = mostRecentServerName;
                    currentServerGroupFound = true;
                    Settings.Value.LastServerGroupCheckup = DateTime.Now;
                }

                //init timers here!
                InitTimers(Settings.Value.ActiveTimers);

                LayoutControl.EnableAddingTimers();
                Settings.DelayedSave();
            }
            catch (Exception _e)
            {
                Logger.LogError("problem updating current server group", this, _e);
            }
        }

        public void Stop()
        {
            Settings.Save();
            WurmLogs.UnsubscribeFromLogFeed(Player, OnNewLogEvents);
            ParentModule.UnregisterTimersGroup(LayoutControl);
            foreach (var timer in WurmTimers)
            {
                timer.Stop();
            }
            LayoutControl.Dispose();
        }

        internal void AddNewTimer()
        {
            // test
            //MeditationTimer timer = new MeditationTimer();
            //timer.Initialize(this, Player, "Meditation", ServerInfo.ServerGroup.Freedom);
            //WurmTimers.Add(timer);
            // get list of all available timers
            var availableTimers = WurmTimerDescriptors.GetUniqueTimers(Settings.Value.ActiveTimers);
            // choose some
            FormChooseTimers ui = new FormChooseTimers(availableTimers, ParentModule.GetModuleUI());
            if (ui.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InitTimers(ui.Result);
            }
        }

        internal void RemoveTimer(WurmTimer timer)
        {
            try
            {
                var type = WurmTimerDescriptors.GetTimerType(timer);
                Settings.Value.ActiveTimers.Remove(type);
            }
            catch (InvalidOperationException _e)
            {
                if (timer is CustomTimer)
                {
                    Logger.LogInfo("there was an issue with removing custom timer, attempting fix now");
                    Settings.Value.ActiveTimers.RemoveWhere(x => timer.NameID == x.NameID);
                    Logger.LogInfo("fixed");
                }
                else Logger.LogError("problem removing timer from active list, ID: " + timer.TimerID, this, _e);
            }
            Settings.DelayedSave();
            WurmTimers.Remove(timer);
            timer.Stop();
        }

        void InitTimers(HashSet<WurmTimerDescriptors.TimerType> timers)
        {
            var timers_copy = timers.ToArray();
            foreach (var timertype in timers_copy)
            {
                // init
                try
                {
                    WurmTimer newTimer = WurmTimerDescriptors.NewTimerFactory(timertype);
                    newTimer.Initialize(this, Player, timertype.ToString(), timertype.Group, timertype.ToCompactString());
                    newTimer.NameID = timertype.NameID; //added to fix removing custom timers
                    WurmTimers.Add(newTimer);
                    Settings.Value.ActiveTimers.Add(timertype);
                }
                catch (InvalidOperationException)
                {
                    Logger.LogInfo("tried to initialize timer that didn't exist any more, skipped");
                    Settings.Value.ActiveTimers.Remove(timertype);
                }
            }
            //save timer list
            Settings.DelayedSave();
        }

        /// <summary>
        /// If custom timer is edited/deleted by user, this should also stop all timers of that type
        /// </summary>
        /// <param name="timertype"></param>
        public void RemoveDeletedCustomTimer(string nameID)
        {
            var WurmTimers_copy = WurmTimers.ToArray();
            foreach (var timer in WurmTimers_copy)
            {
                if (timer.NameID == nameID) RemoveTimer(timer);
            }
        }

        internal void RegisterNewControlTimer(UControlTimerDisplay ControlTimer)
        {
            LayoutControl.RegisterNewTimerDisplay(ControlTimer);
        }

        internal void UnregisterControlTimer(UControlTimerDisplay ControlTimer)
        {
            LayoutControl.UnregisterTimerDisplay(ControlTimer);
        }

        internal void Update(bool engineSleeping)
        {
            Settings.Update();
            foreach (var timer in WurmTimers)
            {
                if (timer.InitCompleted) timer.Update(engineSleeping);
                else if (timer.RunUpdateRegardlessOfInitCompleted) timer.Update(engineSleeping);
            }
        }

        private void OnNewLogEvents(object sender, NewLogEntriesEventArgs e)
        {
            Logger.LogDebug("events received", this);
            foreach (var container in e.Entries.AllEntries)
            {
                if (container.LogType == GameLogTypes.Event)
                {
                    foreach (string line in container.Entries)
                    {
                        try
                        {
                            //detect server travel and update information
                            if (line.Contains("You are on"))
                            {
                                string serverName;
                                ServerInfo.ServerGroup group = WurmLogSearcherAPI.GetServerGroupFromLine(line, out serverName);
                                if (group != ServerInfo.ServerGroup.Unknown)
                                {
                                    if (!String.IsNullOrEmpty(serverName)) Settings.Value.GroupToServerMap[group] = serverName;
                                    CurrentServerGroup = group;
                                    currentServerGroupFound = true;
                                    Settings.Value.LastServerGroupCheckup = DateTime.Now;
                                    Settings.Value.CurrentServerName = serverName;

                                    // this is now handled via WurmState.WurmServer event
                                    //foreach (var timer in WurmTimers)
                                    //{
                                    //    if (timer.TargetServerGroup == group)
                                    //        timer.HandleServerChange();
                                    //}

                                    Settings.DelayedSave();
                                }
                            }
                        }
                        catch (Exception _e)
                        {
                            Logger.LogError("problem parsing line while updating current server group, line: " + line, this, _e);
                        }
                    }

                    foreach (var timer in WurmTimers)
                    {
                        if (timer.InitCompleted && CurrentServerGroup == timer.TargetServerGroup)
                        {
                            foreach (string line in container.Entries)
                            {
                                timer.HandleNewEventLogLine(line);
                            }
                        }
                    }
                }
                else if (container.LogType == GameLogTypes.Skills)
                {
                    //call all timers with wurmskill handler
                    foreach (var timer in WurmTimers)
                    {
                        if (timer.InitCompleted && CurrentServerGroup == timer.TargetServerGroup)
                        {
                            foreach (string line in container.Entries)
                            {
                                timer.HandleNewSkillLogLine(line);
                            }
                        }
                    }
                }
            }

            foreach (var timer in WurmTimers)
            {
                if (timer.InitCompleted && CurrentServerGroup == timer.TargetServerGroup)
                {
                    foreach (var container in e.Entries.AllEntries)
                    {
                        timer.HandleAnyLogLine(container);
                    }
                }
            }
        }

        internal FormTimers GetModuleUI()
        {
            return ParentModule.GetModuleUI();
        }
    }
}
