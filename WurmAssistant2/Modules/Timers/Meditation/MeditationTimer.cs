using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Aldurcraft.WurmOnline.WurmLogsManager;
using Aldurcraft.WurmOnline.WurmLogsManager.Searcher;
using Aldurcraft.WurmOnline.WurmState;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Timers
{
    using ServerInfo = WurmServer.ServerInfo;

    public class MeditationTimer : WurmTimer
    {
        const double FloatAboveZeroCompareValue = 0.00001;

        [DataContract]
        public class MeditTimerSettings
        {
            [DataMember]
            public DateTime MeditSkillLastCheckup;
            [DataMember]
            public float MeditationSkill;
            [DataMember]
            public bool SleepBonusReminder;
            [DataMember]
            public int SleepBonusPopupDuration;
            [DataMember]
            public bool ShowMeditSkill;
            [DataMember]
            public bool ShowMeditCount;

            public MeditTimerSettings()
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
                SleepBonusPopupDuration = 4000;
            }
        }

        enum MeditationStates { Unlimited, Limited }
        enum MeditHistoryEntryTypes { Meditation, LongCooldownTrigger }

        class MeditHistoryEntry
        {
            public MeditHistoryEntryTypes EntryType;
            public DateTime EntryDateTime;
            public bool Valid = false;
            /// <summary>
            /// this is a bandaid fix flag for uptime not reseting medit cooldown
            /// </summary>
            /// <remarks>
            /// due to recent change in wurm, cooldowns do not appear to reset immediatelly after uptime
            /// it is unknown if this is some delay, random issue or the cooldown just simply doesnt reset on uptime swap
            /// this flag can be used to mark an entry, that happened just prior to cooldown reset
            /// in can then be checked and entry returned as cooldown trigger, if no more recent entries are found,
            /// this in effect triggers a cooldown but does not mess the "Valid" flag, which is used to count medits during
            /// single uptime window (so that long/short cooldown count remains correct).
            /// </remarks>
            public bool ThisShouldTriggerCooldownButNotCountForThisUptimeWindow = false;

            public MeditHistoryEntry(MeditHistoryEntryTypes type, DateTime date)
            {
                this.EntryType = type;
                this.EntryDateTime = date;
            }
        }

        static TimeSpan LongMeditCooldown = new TimeSpan(3, 0, 0);
        static TimeSpan ShortMeditCooldown = new TimeSpan(0, 30, 0);

        List<MeditHistoryEntry> MeditHistory = new List<MeditHistoryEntry>();

        DateTime _nextMeditationDate = DateTime.MinValue;
        DateTime NextMeditationDate
        {
            get { return _nextMeditationDate; }
            set { _nextMeditationDate = value; CDNotify.CooldownTo = value; }
        }

        DateTime CooldownResetSince = DateTime.Now;
        TimeSpan TimeOnThisCooldownReset = new TimeSpan(0);
        bool isLongMeditCooldown = false;

        private MeditationStates MeditState = MeditationStates.Unlimited;

        SleepBonusNotify SleepNotify;

        //persist
        //DateTime MeditSkillLastCheckup = DateTime.MinValue;
        //persist
        //float _meditationSkill = 0;

        PersistentObject<MeditTimerSettings> Settings;

        public bool ShowMeditCount
        {
            get { return Settings.Value.ShowMeditCount; }
            set
            {
                TimerDisplay.ShowMeditCount = value;
                Settings.Value.ShowMeditCount = value;
                Settings.DelayedSave();
            }
        }

        DateTime MeditSkillLastCheckup
        {
            get { return Settings.Value.MeditSkillLastCheckup; }
            set
            {
                Settings.Value.MeditSkillLastCheckup = value;
                Settings.DelayedSave();
            }
        }

        float MeditationSkill
        {
            get { return Settings.Value.MeditationSkill; }
            set
            {
                Logger.LogInfo(string.Format("{0} meditation skill is now {1} on {2}", Player, value, TargetServerGroup), this);
                TimerDisplay.UpdateSkill(value);
                if (value < 20f)
                {
                    if (MeditState != MeditationStates.Unlimited)
                    {
                        UpdateMeditationCooldown();
                        MeditState = MeditationStates.Unlimited;
                    }
                }
                else
                {
                    if (MeditState != MeditationStates.Limited)
                    {
                        UpdateMeditationCooldown();
                        MeditState = MeditationStates.Limited;
                    }
                }
                Settings.Value.MeditationSkill = value;
                Settings.DelayedSave();
            }
        }
        public bool SleepBonusReminder
        {
            get { return Settings.Value.SleepBonusReminder; }
            set
            {
                SleepNotify.Enabled = value;
                Settings.Value.SleepBonusReminder = value;
                Settings.DelayedSave();
            }
        }
        /// <summary>
        /// milliseconds
        /// </summary>
        public int SleepBonusPopupDuration
        {
            get { return Settings.Value.SleepBonusPopupDuration; }
            set
            {
                SleepNotify.PopupDuration = value;
                Settings.Value.SleepBonusPopupDuration = value;
                Settings.DelayedSave();
            }
        }

        public override void Initialize(PlayerTimersGroup parentGroup, string player, string timerId, ServerInfo.ServerGroup serverGroup, string compactId)
        {
            base.Initialize(parentGroup, player, timerId, serverGroup, compactId);
            //more inits
            TimerDisplay.SetCooldown(ShortMeditCooldown);
            //load settings
            Settings = new PersistentObject<MeditTimerSettings>(new MeditTimerSettings());
            Settings.FilePath = SettingsSavePath;
            if (!Settings.Load()) Settings.Save();

            SleepNotify = new SleepBonusNotify(Player, "Can turn off sleep bonus now");
            SleepNotify.Enabled = SleepBonusReminder;

            TimerDisplay.UpdateSkill(MeditationSkill);
            TimerDisplay.ShowSkill = Settings.Value.ShowMeditSkill;
            TimerDisplay.ShowMeditCount = Settings.Value.ShowMeditCount;

            MoreOptionsAvailable = true;
            PerformAsyncInits();
        }

        async Task PerformAsyncInits()
        {
            try
            {
                var hasArchivalLevel = MeditationSkill > FloatAboveZeroCompareValue;
                float skill = await FindMeditationSkill(hasArchivalLevel);

                if (skill > FloatAboveZeroCompareValue)
                {
                    MeditationSkill = skill;
                }
                else
                {
                    // forcing update of the timer and "limited" flag
                    MeditationSkill = MeditationSkill;
                }
                MeditSkillLastCheckup = DateTime.Now;

                List<string> historyEntries = await GetLogLinesFromLogHistoryAsync(GameLogTypes.Event, TimeSpan.FromDays(3));
                foreach (string line in historyEntries)
                {
                    if (line.Contains("You finish your meditation"))
                    {
                        DateTime datetime;
                        if (WurmLogSearcherAPI.TryParseDateTimeFromSearchResultLine(line, out datetime))
                        {
                            MeditHistory.Add(new MeditHistoryEntry(MeditHistoryEntryTypes.Meditation, datetime));
                        }
                        else
                        {
                            Logger.LogError("error while parsing date for medit history for " + Player + "from line: " + line, this);
                        }
                    }
                    else if (line.Contains("You feel that it will take you a while before you are ready to meditate again"))
                    {
                        DateTime datetime;
                        if (WurmLogSearcherAPI.TryParseDateTimeFromSearchResultLine(line, out datetime))
                        {
                            MeditHistory.Add(new MeditHistoryEntry(MeditHistoryEntryTypes.LongCooldownTrigger, datetime));
                        }
                        else
                        {
                            Logger.LogError("error while parsing date for medit history for " + Player + "from line: " + line, this);
                        }
                    }
                }

                UpdateMeditationCooldown();

                //now new log events can be handled
                InitCompleted = true;
            }
            catch (Exception _e)
            {
                Logger.LogCritical("problem while preparing timer", this, _e);
            }
        }

        async Task<float> FindMeditationSkill(bool hasArchivalLevel)
        {
            var searchFromDate = DateTime.Now;
            if (MeditSkillLastCheckup > new DateTime(1900, 1, 1))
            {
                searchFromDate = MeditSkillLastCheckup;
            }
            searchFromDate -= TimeSpan.FromDays(30);
            float skill = await GetSkillFromLogHistoryAsync("Meditating", searchFromDate);
            if (skill < FloatAboveZeroCompareValue)
            {
                if (!hasArchivalLevel)
                {
                    Logger.LogInfo("while preparing medit timer for player: " + Player + ", skill appears to be 0, attempting wider search", this);
                    skill = await GetSkillFromLogHistoryAsync("Meditating", TimeSpan.FromDays(365));
                    if (skill < FloatAboveZeroCompareValue)
                    {
                        skill = await GetSkillFromLogHistoryAsync("Meditating", TimeSpan.FromDays(1460));
                        if (skill < FloatAboveZeroCompareValue)
                        {
                            Logger.LogError("could not get any meditation skill for player: " + Player);
                        }
                    }
                }
                else
                {
                    Logger.LogInfo("Archival level available, skipping wider search for player: " + Player, this);
                }
            }
            return skill;
        }

        public override void Update(bool engineSleeping)
        {
            base.Update(engineSleeping);
            Settings.Update();
            SleepNotify.Update();
            if (TimerDisplay.Visible) TimerDisplay.UpdateCooldown(NextMeditationDate - DateTime.Now);
        }

        public override void Stop()
        {
            //cleanup here
            Settings.Save();
            base.Stop();
        }

        //this happens only if new server is of current group
        //this includes when player is coming back to this timer group!
        protected override void HandleServerChange()
        {
            UpdateMeditationCooldown();
        }

        public override void OpenMoreOptions(FormTimerSettingsDefault form)
        {
            MeditationTimerOptions moreOptUI = new MeditationTimerOptions(this, form);
            moreOptUI.ShowDialog();
        }

        //ported 1.x methods

        public override void HandleNewEventLogLine(string line)
        {
            if (line.StartsWith("You finish your meditation", StringComparison.Ordinal))
            {
                MeditHistory.Add(new MeditHistoryEntry(MeditHistoryEntryTypes.Meditation, DateTime.Now));
                UpdateMeditationCooldown();
                SleepNotify.MeditationHappened();
            }
            else if (line.StartsWith("The server has been up", StringComparison.Ordinal)) //"The server has been up 14 hours and 22 minutes."
            {
                //this is no longer needed because of HandleServerChange
                //which fires every time player logs into a server (start new wurm or relog or server travel)
                UpdateMeditationCooldown();
            }
            else if (line.StartsWith("You start using the sleep bonus", StringComparison.Ordinal))
            {
                SleepNotify.SleepBonusActivated();
            }
            else if (line.StartsWith("You refrain from using the sleep bonus", StringComparison.Ordinal))
            {
                SleepNotify.SleepBonusDeactivated();
            }
            //[04:31:56] You feel that it will take you a while before you are ready to meditate again.
            else if (line.StartsWith("You feel that it will take", StringComparison.Ordinal))
            {
                if (line.StartsWith("You feel that it will take you a while before you are ready to meditate again", StringComparison.Ordinal))
                {
                    MeditHistory.Add(new MeditHistoryEntry(MeditHistoryEntryTypes.LongCooldownTrigger, DateTime.Now));
                    UpdateMeditationCooldown();
                }
            }
        }

        void UpdateMeditationCooldown()
        {
            UpdateDateOfLastCooldownReset();
            RevalidateMeditHistory();
            UpdateNextMeditDate();
        }

        public override void HandleNewSkillLogLine(string line)
        {
            if (line.StartsWith("Meditating increased", StringComparison.Ordinal) || line.StartsWith("Meditating decreased", StringComparison.Ordinal))
            {
                //parse into value
                float extractedMeditSkill = GeneralHelper.ExtractSkillLEVELFromLine(line);
                if (extractedMeditSkill > 0)
                {
                    this.MeditationSkill = extractedMeditSkill;
                    Logger.LogInfo("updated meditation skill for " + Player + " to " + MeditationSkill, this);
                }
            }
        }

        void UpdateDateOfLastCooldownReset()
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
                    this.CooldownResetSince = cooldownResetDate;
                }
                else Logger.LogError("could not update server uptime information, "+TargetServerGroup + ", "+Player, this);
            }
            catch (Exception _e)
            {
                Logger.LogError("could not update server uptime information, " + TargetServerGroup + ", " + Player, this, _e);
            }
        }

        void RevalidateMeditHistory()
        {
            DateTime lastValidEntry = new DateTime(0);
            TimeSpan currentCooldownTimeSpan = ShortMeditCooldown;
            int countThisReset = 0;
            //validate entries
            foreach (MeditHistoryEntry entry in MeditHistory)
            {
                entry.Valid = false;

                // all entries are default invalid
                // discard any entry prior to cooldown reset
                if (entry.EntryDateTime > CooldownResetSince)
                {
                    if (entry.EntryType == MeditHistoryEntryTypes.LongCooldownTrigger)
                    {
                        //apply longer cooldown from this point
                        currentCooldownTimeSpan = LongMeditCooldown;
                        this.isLongMeditCooldown = true;
                    }

                    // if entry date is later, than last valid + cooldown period, medit counts for daily cap
                    if (entry.EntryDateTime > lastValidEntry + currentCooldownTimeSpan)
                    {
                        entry.Valid = true;
                        lastValidEntry = entry.EntryDateTime;
                    }
                }
                else if (entry.EntryDateTime > CooldownResetSince - TimeSpan.FromMinutes(30))
                {
                    entry.ThisShouldTriggerCooldownButNotCountForThisUptimeWindow = true;
                }

                if (entry.Valid)
                {
                    countThisReset++;
                }
            }
            TimerDisplay.SetMeditCount(countThisReset);
            // resets medit cooldown type in case long is set from previous uptime period
            if (currentCooldownTimeSpan == ShortMeditCooldown) this.isLongMeditCooldown = false;

            //debug
            //List<string> dumphistory = new List<string>();
            //foreach (MeditHistoryEntry entry in MeditHistory)
            //{
            //    dumphistory.Add(entry.EntryDateTime.ToString() + ", " + entry.EntryType.ToString() + ", " + entry.Valid.ToString());
            //}
            //DebugDump.DumpToTextFile("meditvalidatedlist.txt", dumphistory);
        }

        void UpdateNextMeditDate()
        {
            if (MeditState == MeditationStates.Limited)
            {
                if (isLongMeditCooldown)
                {
                    NextMeditationDate = FindLastValidMeditInHistory() + LongMeditCooldown;
                }
                else
                {
                    NextMeditationDate = FindLastValidMeditInHistory() + ShortMeditCooldown;
                }

                if (NextMeditationDate > CooldownResetSince + TimeSpan.FromDays(1))
                {
                    NextMeditationDate = CooldownResetSince + TimeSpan.FromDays(1);
                }
            }
            else this.NextMeditationDate = DateTime.Now;
        }

        DateTime FindLastValidMeditInHistory()
        {
            if (MeditHistory.Count > 0)
            {
                for (int i = MeditHistory.Count - 1; i >= 0; i--)
                {
                    if (MeditHistory[i].EntryType == MeditHistoryEntryTypes.Meditation)
                    {
                        if (MeditHistory[i].Valid) return MeditHistory[i].EntryDateTime;
                    }
                }
                // due to recent change in wurm, cooldowns do not appear to reset immediatelly after uptime
                // it is 
                // if nothing found, we need to apply cooldown based on any medits just prior to cooldown reset
                // currently it doesnt care if last medit was short or long cooldown and always applies 30 min
                // this may need adjustment after proper testing, which frankly is a pain in the ass to do
                for (int i = MeditHistory.Count - 1; i >= 0; i--)
                {
                    if (MeditHistory[i].EntryType == MeditHistoryEntryTypes.Meditation)
                    {
                        if (MeditHistory[i].ThisShouldTriggerCooldownButNotCountForThisUptimeWindow) return MeditHistory[i].EntryDateTime;
                    }
                }
            }
            return new DateTime(0);
        }

        class SleepBonusNotify
        {
            NotifyHandler Handler;

            DateTime? SleepBonusStarted = null;
            DateTime? LastMeditHappenedOn = DateTime.MinValue;

            bool meditHappened = false;

            public bool Enabled { get; set; }
            public int PopupDuration { get { return Handler.Duration; } set { Handler.Duration = value; } }

            public SleepBonusNotify(string popupTitle, string popupMessage, bool popupPersistent = false)
            {
                Handler = new NotifyHandler();
                Handler.PopupPersistent = popupPersistent;
                Handler.Message = (popupMessage ?? "");
                Handler.Title = (popupTitle ?? "");
            }

            public void Update()
            {
                if (SleepBonusStarted != null && meditHappened && DateTime.Now > SleepBonusStarted + TimeSpan.FromMinutes(5))
                {
                    if (LastMeditHappenedOn > DateTime.Now - TimeSpan.FromMinutes(10))
                    {
                        if (Enabled) Handler.Show();
                    }
                    meditHappened = false;
                }
                Handler.Update();
            }

            public void SleepBonusActivated()
            {
                meditHappened = false;
                SleepBonusStarted = DateTime.Now;
            }

            public void SleepBonusDeactivated()
            {
                SleepBonusStarted = null;
                meditHappened = false;
            }

            public void MeditationHappened()
            {
                LastMeditHappenedOn = DateTime.Now;
                meditHappened = true;
            }
        }

        public bool ShowMeditSkill
        {
            get
            {
                return Settings.Value.ShowMeditSkill;
            }
            set
            {
                Settings.Value.ShowMeditSkill = value;
                Settings.DelayedSave();
                TimerDisplay.ShowSkill = value;
            }
        }
    }
}
