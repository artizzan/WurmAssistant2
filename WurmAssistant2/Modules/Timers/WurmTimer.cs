using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using Aldurcraft.WurmOnline.WurmState;
using Aldurcraft.WurmOnline.WurmLogsManager;
using Aldurcraft.WurmOnline.WurmLogsManager.Searcher;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Timers
{
    using ServerInfo = Aldurcraft.WurmOnline.WurmState.WurmServer.ServerInfo;

    public static class WurmTimerDescriptors
    {
        public class RemovedTimerEventArgs : EventArgs
        {
            public string NameID { get; private set; }
            public RemovedTimerEventArgs(string nameID)
            {
                NameID = nameID;
            }
        }

        public static event EventHandler<RemovedTimerEventArgs> RemovedCustomTimer;

        [DataContract] //set of options used to init custom timers behavior
        public class CustomTimerOptions
        {
            public struct Condition
            {
                public string RegexPattern;
                public GameLogTypes LogType;
            }
            [DataMember]
            public Condition[] TriggerConditions;
            [DataMember]
            public Condition[] ResetConditions;
            [DataMember]
            public bool ResetOnUptime;
            [DataMember]
            public TimeSpan Duration;
            [DataMember]
            public bool IsRegex { get; private set; }

            Condition ConditionFactory(string pattern, GameLogTypes logtype, bool isRegex)
            {
                if (pattern == null) pattern = "";
                pattern = pattern.Trim();
                if (!isRegex) pattern = Regex.Escape(pattern);
                else IsRegex = true;

                Condition cond = new Condition();
                cond.RegexPattern = pattern;
                cond.LogType = logtype;
                return cond;
            }

            public void AddTrigger(string pattern, GameLogTypes logtype, bool isRegex)
            {
                var cond = ConditionFactory(pattern, logtype, isRegex);
                if (TriggerConditions != null)
                {
                    var x = TriggerConditions.ToList();
                    x.Add(cond);
                    TriggerConditions = x.ToArray();
                }
                else TriggerConditions = new Condition[] { cond };
            }

            public void AddReset(string pattern, GameLogTypes logtype, bool isRegex)
            {
                var cond = ConditionFactory(pattern, logtype, isRegex);
                if (ResetConditions != null)
                {
                    var x = ResetConditions.ToList();
                    x.Add(ConditionFactory(pattern, logtype, isRegex));
                    ResetConditions = x.ToArray();
                }
                else ResetConditions = new Condition[] { cond };
            }
        }

        [DataContract] //timer is unique if its group and nameID are identical
        public struct TimerType
        {
            [DataMember]
            public string NameID;
            [DataMember]
            public ServerInfo.ServerGroup Group;
            [DataMember]
            public Type UnderlyingTimerType;
            [DataMember]
            public CustomTimerOptions Options;

            public TimerType(string name, ServerInfo.ServerGroup group, Type underlyingType, CustomTimerOptions options)
            {
                NameID = name;
                Group = group;
                UnderlyingTimerType = underlyingType;
                Options = options;
            }

            public bool Equals(TimerType other)
            {
                return NameID == other.NameID && Group == other.Group;
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (obj is TimerType)
                {
                    TimerType other = (TimerType)obj;
                    return NameID == other.NameID && Group == other.Group;
                }
                else return false;
            }

            public override int GetHashCode()
            {
                return unchecked(NameID.GetHashCode() + Group.GetHashCode());
            }

            public override string ToString()
            {
                return NameID + " (" + Group.ToString() + ")";
            }

            public string ToCompactString()
            {
                return string.Format("{0} ({1})", NameID, Group.ToString().Substring(0, 1));
            }
        }

        /// <summary>
        /// readonly! do not modify except with appropriate methods!
        /// </summary>
        public static HashSet<TimerType> Descriptors = new HashSet<TimerType>();

        //static HashSet<TimerType> PersistentCustomTimers = new HashSet<TimerType>();
        static PersistentObject<HashSet<TimerType>> PersistentDescriptors;

        static WurmTimerDescriptors()
        {
            PersistentDescriptors = new PersistentObject<HashSet<TimerType>>(new HashSet<TimerType>());

            //AddDescriptor("Test", typeof(TestTimer));
            AddDescriptor("Meditation", typeof(MeditationTimer));
            AddDescriptor("Path Question", typeof(MeditPathTimer));
            AddDescriptor("Prayer", typeof(PrayerTimer));
            AddDescriptor("Sermon", typeof(SermonTimer));
            AddDescriptor("Alignment", typeof(AlignmentTimer));
            AddDescriptor("Junk Sale", typeof(JunkSaleTimer));
        }

        /// <summary>
        /// adds a new Timer descriptor, which is used to generate list for users to choose from
        /// </summary>
        /// <param name="name">short descriptive name for this timer</param>
        /// <param name="type">typeof(MyTimerClass)</param>
        /// <param name="options">configuration options for CustomTimer</param>
        /// <param name="customTimer">set true to persist custom timer between sessions</param>
        static void AddDescriptor(string name, Type type, CustomTimerOptions options = null, bool customTimer = false)
        {
            foreach (var group in ServerInfo.GetAllServerGroups())
            {
                TimerType timertype = new TimerType(name, group, type, options);
                Descriptors.Add(timertype);
                if (customTimer) PersistentDescriptors.Value.Add(timertype);
            }
            if (customTimer) PersistentDescriptors.Save();
        }

        //timers are kept in hashset and identitcal TimerType are not duplicated on multiple adds
        public static void LoadCustomTimers(string path)
        {
            PersistentDescriptors.SetFilePathAndLoad(path);

            foreach (var timer in PersistentDescriptors.Value)
            {
                Descriptors.Add(timer);
            }
        }

        public static void AddCustomTimer(string nameID, CustomTimerOptions options)
        {
            AddDescriptor(nameID, typeof(CustomTimer), options, true);
        }

        /// <summary>
        /// Name refers to TimerID string, without server group
        /// </summary>
        /// <param name="nameID"></param>
        public static void RemoveCustomTimer(string nameID)
        {
            Descriptors.RemoveWhere(x => x.NameID == nameID);
            PersistentDescriptors.Value.RemoveWhere(x => x.NameID == nameID);
            PersistentDescriptors.Save();
            if (RemovedCustomTimer != null) RemovedCustomTimer(new object(), new RemovedTimerEventArgs(nameID));
        }

        public static CustomTimerOptions GetOptionsForTimer(string nameID)
        {
            return Descriptors.First(x => x.NameID == nameID).Options;
        }

        public static string[] GetCustomTimers()
        {
            HashSet<string> uniqueTimerNames = new HashSet<string>();
            foreach (var timer in PersistentDescriptors.Value)
            {
                uniqueTimerNames.Add(timer.NameID);
            }
            return uniqueTimerNames.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timer"></param>
        /// <exception cref="InvalidOperationException">Descriptors did not contain type of this timer</exception>
        /// <returns></returns>
        public static TimerType GetTimerType(WurmTimer timer)
        {
            return Descriptors.First(x => x.NameID == timer.NameID && x.Group == timer.TargetServerGroup);
        }

        public static HashSet<TimerType> GetUniqueTimers(HashSet<TimerType> currentActiveTimers)
        {
            HashSet<TimerType> result = new HashSet<TimerType>();
            foreach (var item in Descriptors)
            {
                if (!currentActiveTimers.Contains(item)) result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// Activate new timer based on provided type. Timer needs to be Initialized() afterwards
        /// </summary>
        /// <param name="timerType"></param>
        /// <exception cref="InvalidOperationException">this timer type does not exist any more in main Descriptor set</exception>
        /// <returns></returns>
        public static WurmTimer NewTimerFactory(TimerType timerType)
        {
            if (!Descriptors.Contains(timerType)) throw new InvalidOperationException("this timer type does not exist any more");

            object newTimer = Activator.CreateInstance(timerType.UnderlyingTimerType);
            var timer = newTimer as CustomTimer;
            if (timer != null)
            {
                // this needs to obtain latest options set, because custom timers 
                // may be readded/edited after timertype was persisted elsewhere
                var latestTimerTypeOptions = Descriptors.First(timerType.Equals).Options;
                timer.ApplyCustomTimerOptions(latestTimerTypeOptions);
            }
            return (WurmTimer)newTimer;
        }

        public static bool IsThisNameIDUnique(string nameID)
        {
            return Descriptors.All(x => x.NameID != nameID);
        }
    }

    public class WurmTimer
    {
        [DataContract]
        public class TimerDefaultSettings
        {
            [DataMember]
            public bool SoundNotify = false;
            [DataMember]
            public bool PopupNotify = false;
            [DataMember]
            public bool PersistentPopup = false;
            [DataMember]
            public string SoundName = null;
            [DataMember]
            public bool PopupOnWALaunch = false;
            [DataMember]
            public int PopupDuration;
            [DataMember] 
            public TimeSpan lastUptimeSnapshot;

            [OnDeserializing]
            void Init(StreamingContext context)
            {
                PopupDuration = 4000;
            }

            public TimerDefaultSettings()
            {
                Init(new StreamingContext());
            }
        }

        public bool SoundNotify
        {
            get { return DefaultSettings.Value.SoundNotify; }
            set
            {
                CDNotify.SoundEnabled = value;
                DefaultSettings.Value.SoundNotify = value;
                DefaultSettings.DelayedSave();
            }
        }
        public bool PopupNotify
        {
            get { return DefaultSettings.Value.PopupNotify; }
            set
            {
                CDNotify.PopupEnabled = value;
                DefaultSettings.Value.PopupNotify = value;
                DefaultSettings.DelayedSave();
            }
        }
        /// <summary>
        /// Duration of the popup in milliseconds
        /// </summary>
        public int PopupDuration
        {
            get { return DefaultSettings.Value.PopupDuration; }
            set
            {
                CDNotify.Duration = value;
                DefaultSettings.Value.PopupDuration = value;
                DefaultSettings.DelayedSave();
            }
        }
        public bool PersistentPopup
        {
            get { return DefaultSettings.Value.PersistentPopup; }
            set
            {
                CDNotify.PersistPopup = value;
                DefaultSettings.Value.PersistentPopup = value;
                DefaultSettings.DelayedSave();
            }
        }
        public string SoundName
        {
            get { return DefaultSettings.Value.SoundName; }
            set
            {
                CDNotify.SoundName = value;
                DefaultSettings.Value.SoundName = value;
                DefaultSettings.DelayedSave();
            }
        }
        public bool PopupOnWALaunch
        {
            get { return DefaultSettings.Value.PopupOnWALaunch; }
            set
            {
                DefaultSettings.Value.PopupOnWALaunch = value;
                DefaultSettings.DelayedSave();
            }
        }

        protected UControlTimerDisplay TimerDisplay;
        protected PlayerTimersGroup ParentGroup;
        protected string Player;

        /// <summary>
        /// added to fix custom timer deletion, this is set AFTER Initialize finishes and should not be relied upon
        /// </summary>
        public string NameID { get; set; }

        public string TimerID { get; private set; }
        public string TimerShortID { get; private set; }
        public ServerInfo.ServerGroup TargetServerGroup { get; private set; }

        /// <summary>
        /// set true in derived timer until its fully initialized and ready to receive updates
        /// </summary>
        public bool InitCompleted = false;
        /// <summary>
        /// set true to run update on this timer regardless if InitCompleted is flagged true
        /// </summary>
        public bool RunUpdateRegardlessOfInitCompleted = false;

        protected CooldownHandler CDNotify;

        /// <summary>
        /// use this path to save custom Settings xml file
        /// </summary>
        protected string SettingsSavePath { get; private set; }

        /// <summary>
        /// settings for this timer, for default CDNotify
        /// </summary>
        public PersistentObject<TimerDefaultSettings> DefaultSettings;

        public virtual void Initialize(PlayerTimersGroup parentGroup, string player, string timerId, ServerInfo.ServerGroup serverGroup, string compactId)
        {
            //derived must call this base before their own inits!
            this.TimerID = timerId;
            this.TimerShortID = compactId;
            this.ParentGroup = parentGroup;
            this.Player = player;
            this.TargetServerGroup = serverGroup;

            SettingsSavePath = Path.Combine(
                ParentGroup.ThisGroupDir,
                TimerID.Replace(" ", "").Replace("(", "_").Replace(")", "") + ".xml");
            DefaultSettings = new PersistentObject<TimerDefaultSettings>(new TimerDefaultSettings());
            DefaultSettings.FilePath = SettingsSavePath.Replace(".xml", "_default.xml");
            if (!DefaultSettings.Load()) DefaultSettings.Save();

            TimerDisplay = new UControlTimerDisplay(this);
            ParentGroup.RegisterNewControlTimer(TimerDisplay);

            CDNotify = new CooldownHandler();
            CDNotify.Duration = DefaultSettings.Value.PopupDuration;
            CDNotify.Title = Player;
            CDNotify.Message = timerId + " cooldown finished";
            CDNotify.SoundEnabled = DefaultSettings.Value.SoundNotify;
            CDNotify.PopupEnabled = DefaultSettings.Value.PopupNotify;
            CDNotify.PersistPopup = DefaultSettings.Value.PersistentPopup;
            CDNotify.SoundName = DefaultSettings.Value.SoundName;
            if (PopupOnWALaunch) CDNotify.ResetShownAndPlayed();

            Aldurcraft.WurmOnline.WurmState.WurmServer.OnPotentialWurmDateAndUptimeChange += _handleServerChange;
        }

        public virtual void Stop()
        {
            //derived should call this base AFTER their own cleanup
            DefaultSettings.Save();
            ParentGroup.UnregisterControlTimer(TimerDisplay);
            TimerDisplay.Dispose();
            Aldurcraft.WurmOnline.WurmState.WurmServer.OnPotentialWurmDateAndUptimeChange -= _handleServerChange;
        }

        public virtual void Update(bool engineSleeping)
        {
            //derived can call this base at any point
            DefaultSettings.Update();
            CDNotify.Update(engineSleeping);
        }

        /// <summary>
        /// Returns 0 if no finds or error, filters out other server groups,
        /// min/max search date is unbound using this overload
        /// </summary>
        /// <param name="skillName">case sens</param>
        /// <param name="since"></param>
        /// <returns></returns>
        protected async Task<float> GetSkillFromLogHistoryAsync(string skillName, TimeSpan since)
        {
            float meditskill = await WurmLogSearcherAPI.GetSkillForPlayerForServerGroupAsync(
                Player, (int)since.TotalDays, skillName, TargetServerGroup);

            return meditskill;
        }

        /// <summary>
        /// Returns 0 if no finds or error, filters out other server groups
        /// min search 7 days ago, max search 120 days ago
        /// </summary>
        /// <param name="skillName">case sens</param>
        /// <param name="lastCheckup"></param>
        /// <returns></returns>
        protected async Task<float> GetSkillFromLogHistoryAsync(string skillName, DateTime lastCheckup)
        {
            float meditskill = await WurmLogSearcherAPI.GetSkillForPlayerForServerGroupAsync(
                Player, (int)HowLongAgoWasThisDate(lastCheckup).TotalDays, skillName, TargetServerGroup);

            return meditskill;
        }

        /// <summary>
        /// returns null on error, filters out other server groups,
        /// min/max search date is unbound using this overload
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="since">amount of time to look back, will accept any value</param>
        /// <returns></returns>
        protected async Task<List<string>> GetLogLinesFromLogHistoryAsync(GameLogTypes logType, TimeSpan since)
        {
            LogSearchData lgs = new LogSearchData();
            lgs.SetSearchCriteria(
                Player,
                logType,
                DateTime.Now - since,
                DateTime.Now,
                "",
                SearchTypes.RegexEscapedCaseIns);

            lgs = await WurmLogSearcherAPI.SearchWurmLogsFilteredByServerGroupAsync(lgs, TargetServerGroup);

            return lgs.AllLines;
        }

        /// <summary>
        /// returns null on error, filters out other server groups
        /// min search 7 days ago, max search 120 days ago
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="lastCheckup"></param>
        /// <returns></returns>
        protected async Task<List<string>> GetLogLinesFromLogHistoryAsync(GameLogTypes logType, DateTime lastCheckup)
        {
            return await GetLogLinesFromLogHistoryAsync(logType, HowLongAgoWasThisDate(lastCheckup));
        }

        private TimeSpan HowLongAgoWasThisDate(DateTime lastCheckup)
        {
            TimeSpan timeNotChecked = DateTime.Now - lastCheckup - TimeSpan.FromDays(1);

            if (timeNotChecked.TotalDays < 7) timeNotChecked = TimeSpan.FromDays(7);
            else if (timeNotChecked.TotalDays > 120) timeNotChecked = TimeSpan.FromDays(120);

            return timeNotChecked;
        }

        /// <summary>
        /// override to handle new wurm events in Event log
        /// </summary>
        /// <param name="line"></param>
        public virtual void HandleNewEventLogLine(string line)
        {

        }

        /// <summary>
        /// override to handle new wurm events in Skills log
        /// </summary>
        /// <param name="line"></param>
        public virtual void HandleNewSkillLogLine(string line)
        {

        }

        public virtual void HandleAnyLogLine(NewLogEntriesContainer container)
        {

        }

        private void _handleServerChange(object sender, EventArgs e)
        {
            HandleServerChange();
        }

        /// <summary>
        /// triggered after wurm date and uptime info was refreshed
        /// </summary>
        protected virtual void HandleServerChange()
        {

        }

        /// <summary>
        /// opens config for default timer settings
        /// </summary>
        public void OpenTimerConfig()
        {
            FormTimerSettingsDefault ui = new FormTimerSettingsDefault(this);
            ui.ShowDialog();
        }

        /// <summary>
        /// set true to enable "more options" button in default timer settings
        /// </summary>
        public bool MoreOptionsAvailable { get; set; }

        public PersistentObject<ModuleTimers.TimersSettings> GlobalSettings { get { return ParentGroup.GlobalSettings; } }

        /// <summary>
        /// override to show "more options" window
        /// </summary>
        /// <param name="form"></param>
        public virtual void OpenMoreOptions(FormTimerSettingsDefault form)
        {
        }

        /// <summary>
        /// returns ref to main module Form
        /// </summary>
        /// <returns></returns>
        public FormTimers GetModuleUI()
        {
            return ParentGroup.GetModuleUI();
        }

        /// <summary>
        /// closes and disposes of this timer
        /// </summary>
        internal void TurnOff()
        {
            ParentGroup.RemoveTimer(this);
        }

        protected DateTime GetLatestUptimeCooldownResetDate()
        {
            try
            {
                DateTime currentTime = DateTime.Now;
                DateTime cooldownResetDate = currentTime;

                TimeSpan serverUptime;
                if (WurmServer.TryGetUptime(ParentGroup.Settings.Value.GroupToServerMap[TargetServerGroup], out serverUptime))
                {
                    TimeSpan timeSinceLastServerReset = serverUptime;
                    TimeSpan daysSinceLastServerReset = new TimeSpan(timeSinceLastServerReset.Days, 0, 0, 0);
                    timeSinceLastServerReset = timeSinceLastServerReset.Subtract(daysSinceLastServerReset);

                    cooldownResetDate = currentTime - timeSinceLastServerReset;
                    return cooldownResetDate;
                }
                else
                {
                    Logger.LogInfo(string.Format("could not get server uptime, timerID: {0}, group: {1}, server: {2}, player: {3}", 
                        TimerID, TargetServerGroup, ParentGroup.CurrentServerName, Player), this);
                    return DateTime.MinValue;
                }
            }
            catch (Exception _e)
            {
                Logger.LogInfo(string.Format("could not get server uptime, timerID: {0}, group: {1}, server: {2}, player: {3}",
                    TimerID, TargetServerGroup, ParentGroup.CurrentServerName, Player), this, _e);
                return DateTime.MinValue;
            }
        }
    }
}
