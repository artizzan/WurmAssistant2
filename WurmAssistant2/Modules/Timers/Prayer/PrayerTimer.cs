using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Aldurcraft.WurmOnline.WurmLogsManager.Searcher;
using Aldurcraft.WurmOnline.WurmState;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Timers
{
    public class PrayerTimer : WurmTimer
    {
        [DataContract]
        public class PrayerTimerSettings
        {
            [DataMember]
            public float FaithLevel = 0;
            [DataMember]
            public DateTime FaithLevelLastCheckup = DateTime.MinValue;
            [DataMember]
            public FavorTimerNotify.FavorTimerNotifySettings FavorSettings = new FavorTimerNotify.FavorTimerNotifySettings();
            [DataMember]
            public bool ShowFaithSkill;

            [OnDeserialized]
            void FixMe(StreamingContext context)
            {
                if (FavorSettings == null) FavorSettings = new FavorTimerNotify.FavorTimerNotifySettings();
            }
        }

        enum PrayHistoryEntryTypes { Prayed, SermonMightyPleased, FaithGainBelow120, FaithGain120orMore }

        class PrayHistoryEntry : IComparable<PrayHistoryEntry>
        {
            public PrayHistoryEntryTypes EntryType;
            public DateTime EntryDateTime;
            public bool Valid = false;

            public PrayHistoryEntry(PrayHistoryEntryTypes type, DateTime date)
            {
                this.EntryType = type;
                this.EntryDateTime = date;
            }

            public int CompareTo(PrayHistoryEntry dtlm)
            {
                return this.EntryDateTime.CompareTo(dtlm.EntryDateTime);
            }
        }

        public static TimeSpan PrayCooldown = new TimeSpan(0, 20, 0);
        List<PrayHistoryEntry> PrayerHistory = new List<PrayHistoryEntry>();
        DateTime CooldownResetSince = DateTime.MinValue;

        DateTime _nextPrayDate = DateTime.MinValue;
        DateTime NextPrayDate
        {
            get { return _nextPrayDate; }
            set
            {
                _nextPrayDate = value; 
                CDNotify.CooldownTo = value;
                if (isPrayCountMax) TimerDisplay.ExtraInfo = " (max)";
                else TimerDisplay.ExtraInfo = null;
            }
        }

        bool isPrayCountMax = false;

        public PersistentObject<PrayerTimerSettings> Settings;

        FavorTimerNotify FavorNotify;

        float FaithLevel
        {
            get { return Settings.Value.FaithLevel; }
            set
            {
                Settings.Value.FaithLevel = value;
                FavorNotify.CurrentFavorMAX = value;
                TimerDisplay.UpdateSkill(value);
                Settings.DelayedSave();
                Logger.LogInfo(string.Format("{0} faith level is now {1} on {2}", Player, value, TargetServerGroup), this);
            }
        }
        DateTime FaithLevelLastCheckup
        {
            get { return Settings.Value.FaithLevelLastCheckup; }
            set
            {
                Settings.Value.FaithLevelLastCheckup = value;
                Settings.DelayedSave();
            }
        }

        public bool ShowFaithSkillOnTimer
        {
            get
            {
                return Settings.Value.ShowFaithSkill;
            }
            set
            {
                Settings.Value.ShowFaithSkill = value;
                TimerDisplay.ShowSkill = value;
                Settings.DelayedSave();
            }
        }

        public override void Initialize(PlayerTimersGroup parentGroup, string player, string timerId, Aldurcraft.WurmOnline.WurmState.WurmServer.ServerInfo.ServerGroup serverGroup, string compactId)
        {
            base.Initialize(parentGroup, player, timerId, serverGroup, compactId);
            TimerDisplay.SetCooldown(PrayCooldown);
            MoreOptionsAvailable = true;

            Settings = new PersistentObject<PrayerTimerSettings>(new PrayerTimerSettings());
            Settings.SetFilePathAndLoad(SettingsSavePath);

            FavorNotify = new FavorTimerNotify(Settings.Value.FavorSettings, Player, TargetServerGroup);

            TimerDisplay.UpdateSkill(FaithLevel);
            TimerDisplay.ShowSkill = ShowFaithSkillOnTimer;

            PerformAsyncInits();
        }

        async Task PerformAsyncInits()
        {
            try
            {
                float skill = await GetSkillFromLogHistoryAsync("Faith", FaithLevelLastCheckup);
                if (skill > 0)
                {
                    FaithLevel = skill;
                }
                else if (FaithLevel == 0)
                {
                    Logger.LogInfo("faith was 0 while preparing prayer timer for player: " + Player + ". Attempting 1-year thorough search", this);
                    skill = await GetSkillFromLogHistoryAsync("Faith", TimeSpan.FromDays(365));
                    if (skill > 0)
                    {
                        FaithLevel = skill;
                    }
                    else Logger.LogInfo("faith still 0, giving up, this may be a bug or no faith gained yet");
                }
                FaithLevelLastCheckup = DateTime.Now;

                List<string> lines = await GetLogLinesFromLogHistoryAsync(Aldurcraft.WurmOnline.WurmLogsManager.GameLogTypes.Skills, TimeSpan.FromDays(3));

                foreach (string line in lines)
                {
                    if (line.Contains("Faith increased"))
                    {
                        float faithskillgain = GeneralHelper.ExtractSkillGAINFromLine(line);
                        if (faithskillgain > 0)
                        {
                            DateTime datetime;
                            if (WurmLogSearcherAPI.TryParseDateTimeFromSearchResultLine(line, out datetime))
                            {
                                if (faithskillgain >= 0.120F)
                                {
                                    PrayerHistory.Add(new PrayHistoryEntry(PrayHistoryEntryTypes.FaithGain120orMore, datetime));
                                }
                                else
                                {
                                    PrayerHistory.Add(new PrayHistoryEntry(PrayHistoryEntryTypes.FaithGainBelow120, datetime));
                                }
                            }
                        }
                    }
                }

                lines = await GetLogLinesFromLogHistoryAsync(Aldurcraft.WurmOnline.WurmLogsManager.GameLogTypes.Event, TimeSpan.FromDays(3));

                foreach (string line in lines)
                {
                    if (line.Contains("You finish your prayer"))
                    {
                        DateTime datetime;
                        if (WurmLogSearcherAPI.TryParseDateTimeFromSearchResultLine(line, out datetime))
                        {
                            PrayerHistory.Add(new PrayHistoryEntry(PrayHistoryEntryTypes.Prayed, datetime));
                        }
                        else
                        {
                            Logger.LogError("error while parsing date in EndUpdatePrayHistoryCache", this);
                        }
                    }
                    else if (line.Contains("is mighty pleased with you"))
                    {
                        DateTime datetime;
                        if (WurmLogSearcherAPI.TryParseDateTimeFromSearchResultLine(line, out datetime))
                        {
                            PrayerHistory.Add(new PrayHistoryEntry(PrayHistoryEntryTypes.SermonMightyPleased, datetime));
                        }
                        else
                        {
                            Logger.LogError("error while parsing date in EndUpdatePrayHistoryCache", this);
                        }
                    }
                }

                UpdatePrayerCooldown();

                InitCompleted = true;
            }
            catch (Exception _e)
            {
                Logger.LogError("init problem", this, _e);
            }
        }

        public void ForceUpdateFavorNotify(string soundName = null, bool? popupPersistent = null)
        {
            FavorNotify.ForceUpdateNotifyHandler(soundName, popupPersistent);
        }

        public override void Update(bool engineSleeping)
        {
            base.Update(engineSleeping);
            Settings.Update();
            if (TimerDisplay.Visible) TimerDisplay.UpdateCooldown(NextPrayDate);
            FavorNotify.Update();
        }

        public override void Stop()
        {
            Settings.Save();
            base.Stop();
        }

        protected override void HandleServerChange()
        {
            UpdatePrayerCooldown();
        }

        public override void OpenMoreOptions(FormTimerSettingsDefault form)
        {
            PrayerTimerOptions ui = new PrayerTimerOptions(this, form);
            ui.ShowDialog();
        }

        public override void HandleNewEventLogLine(string line)
        {
            if (line.StartsWith("You finish your prayer", StringComparison.Ordinal))
            {
                PrayerHistory.Add(new PrayHistoryEntry(PrayHistoryEntryTypes.Prayed, DateTime.Now));
                UpdatePrayerCooldown();
            }
            else if (line.StartsWith("The server has been up", StringComparison.Ordinal)) //"The server has been up 14 hours and 22 minutes."
            {
                UpdatePrayerCooldown();
            }
            else if (line.Contains("is mighty pleased with you"))
            {
                PrayerHistory.Add(new PrayHistoryEntry(PrayHistoryEntryTypes.SermonMightyPleased, DateTime.Now));
                UpdatePrayerCooldown();
            }
        }

        public override void HandleNewSkillLogLine(string line)
        {
            FavorNotify.HandleNewSkillLogLine(line);
            // "[02:03:41] Faith increased by 0,124 to 27,020"
            if (line.StartsWith("Faith increased", StringComparison.Ordinal) || line.StartsWith("Faith decreased", StringComparison.Ordinal))
            {
                float faithskillgain = GeneralHelper.ExtractSkillGAINFromLine(line);
                if (faithskillgain > 0)
                {

                    if (faithskillgain >= 0.120F)
                    {
                        PrayerHistory.Add(new PrayHistoryEntry(PrayHistoryEntryTypes.FaithGain120orMore, DateTime.Now));
                    }
                    else
                    {
                        PrayerHistory.Add(new PrayHistoryEntry(PrayHistoryEntryTypes.FaithGainBelow120, DateTime.Now));
                    }
                }
                float extractedFaithSkill = GeneralHelper.ExtractSkillLEVELFromLine(line);
                if (extractedFaithSkill > 0)
                {
                    FaithLevel = extractedFaithSkill;
                }
                UpdatePrayerCooldown();
            }
        }

        void UpdatePrayerCooldown()
        {
            UpdateDateOfLastCooldownReset();
            RevalidateFaithHistory();
            UpdateNextPrayerDate();
        }

        void UpdateDateOfLastCooldownReset()
        {
            var result = GetLatestUptimeCooldownResetDate();
            if (result > DateTime.MinValue) CooldownResetSince = result;
        }

        void RevalidateFaithHistory()
        {
            //sort the history based on entry datetimes
            PrayerHistory.Sort();

            DateTime lastValidEntry = new DateTime(0);
            PrayHistoryEntry lastMayorSkillGain = null,
                //lastMinorSkillGain = null, //useless because very small ticks will never be logged regardless of setting
                lastMightyPleased = null,
                lastPrayer = null;
            TimeSpan currentPrayCooldownTimeSpan = PrayCooldown;
            int validPrayerCount = 0;
            this.isPrayCountMax = false;
            for (int i = 0; i < PrayerHistory.Count; i++)
            {
                PrayHistoryEntry entry = PrayerHistory[i];
                entry.Valid = false;

                if (entry.EntryDateTime > CooldownResetSince)
                {
                    if (entry.EntryType == PrayHistoryEntryTypes.Prayed) lastPrayer = entry;
                    //else if (entry.EntryType == PrayHistoryEntryTypes.FaithGainBelow120) lastMinorSkillGain = entry;
                    else if (entry.EntryType == PrayHistoryEntryTypes.SermonMightyPleased) lastMightyPleased = entry;
                    else if (entry.EntryType == PrayHistoryEntryTypes.FaithGain120orMore) lastMayorSkillGain = entry;

                    //on sermon event, check if recently there was big faith skill gain, if yes reset prayers
                    if (entry.EntryType == PrayHistoryEntryTypes.SermonMightyPleased)
                    {
                        if (lastMayorSkillGain != null
                            && lastMayorSkillGain.EntryDateTime > entry.EntryDateTime - TimeSpan.FromSeconds(15))
                        {
                            validPrayerCount = 0;
                            this.isPrayCountMax = false;
                        }
                    }
                    //on big faith skill gain, check if recently there was a sermon event, if yes reset prayers
                    else if (entry.EntryType == PrayHistoryEntryTypes.FaithGain120orMore)
                    {
                        if (lastMightyPleased != null
                            && lastMightyPleased.EntryDateTime > entry.EntryDateTime - TimeSpan.FromSeconds(15))
                        {
                            validPrayerCount = 0;
                            this.isPrayCountMax = false;
                        }
                    }
                    //on prayed, if prayer cap not reached, check if it's later than last valid prayer + cooldown, if yes, validate
                    else if (!this.isPrayCountMax
                        && entry.EntryType == PrayHistoryEntryTypes.Prayed
                        && entry.EntryDateTime > lastValidEntry + currentPrayCooldownTimeSpan)
                    {
                        entry.Valid = true;
                        validPrayerCount++;
                        lastValidEntry = entry.EntryDateTime;
                    }

                    //if prayer cap reached, set flag
                    if (validPrayerCount >= 5)
                    {
                        this.isPrayCountMax = true;
                    }
                }
            }
        }

        void UpdateNextPrayerDate()
        {
            if (isPrayCountMax)
            {
                NextPrayDate = CooldownResetSince + TimeSpan.FromDays(1);
            }
            else
            {
                NextPrayDate = FindLastValidPrayerInHistory() + PrayCooldown;
            }

            if (NextPrayDate > CooldownResetSince + TimeSpan.FromDays(1))
            {
                NextPrayDate = CooldownResetSince + TimeSpan.FromDays(1);
            }
        }

        DateTime FindLastValidPrayerInHistory()
        {
            if (PrayerHistory.Count > 0)
            {
                for (int i = PrayerHistory.Count - 1; i >= 0; i--)
                {
                    if (PrayerHistory[i].EntryType == PrayHistoryEntryTypes.Prayed)
                    {
                        if (PrayerHistory[i].Valid) return PrayerHistory[i].EntryDateTime;
                    }
                }
            }
            return new DateTime(0);
        }


        public class FavorTimerNotify
        {
            [DataContract]
            public class FavorTimerNotifySettings
            {
                [DataMember]
                public bool FavorNotifySound = false;
                [DataMember]
                public bool FavorNotifyPopup = false;
                [DataMember]
                public string FavorNotifySoundName = null;
                [DataMember]
                public bool FavorNotifyWhenMAX = false;
                [DataMember]
                public float FavorNotifyOnLevel = 0;
                [DataMember]
                public bool FavorNotifyPopupPersist = false;

                [Obsolete] //moved to faith timer settings
                public bool ShowFaithSkill;
            }

            public FavorTimerNotifySettings Settings { get; private set; }

            NotifyHandler FavorHandler = new NotifyHandler();

            public float CurrentFavorMAX { get; set; }

            float _currentFavorLevel = 0;
            float CurrentFavorLevel
            {
                get { return _currentFavorLevel; }
                set
                {
                    CheckIfNotify(_currentFavorLevel, value);
                    _currentFavorLevel = value;
                }
            }

            WurmServer.ServerInfo.ServerGroup Group;

            public FavorTimerNotify(FavorTimerNotifySettings favorTimerNotifySettings, string player, WurmServer.ServerInfo.ServerGroup group)
            {
                Group = group;
                Settings = favorTimerNotifySettings;
                FavorHandler = new NotifyHandler(
                    Settings.FavorNotifySoundName,
                    player,
                    "",
                    Settings.FavorNotifyPopupPersist);
            }

            public void ForceUpdateNotifyHandler(string soundName = null, bool? persistPopup = null)
            {
                if (soundName != null)
                {
                    FavorHandler.SoundName = soundName;
                }
                if (persistPopup != null)
                {
                    FavorHandler.PopupPersistent = persistPopup.Value;
                }
            }

            void CheckIfNotify(float oldFavor, float newFavor)
            {
                if (Settings.FavorNotifySound || Settings.FavorNotifyPopup)
                {
                    bool notify = false;
                    if (Settings.FavorNotifyWhenMAX)
                    {
                        if (oldFavor < CurrentFavorMAX && newFavor > CurrentFavorMAX) notify = true;
                    }
                    else if (oldFavor < Settings.FavorNotifyOnLevel && newFavor > Settings.FavorNotifyOnLevel) notify = true;
                    if (notify)
                    {
                        if (Settings.FavorNotifySound) FavorHandler.Play();
                        if (Settings.FavorNotifyPopup)
                        {
                            FavorHandler.Message = string.Format("Favor on {0} reached {1}", this.Group, newFavor);
                            FavorHandler.Show();
                        }
                    }
                }
            }

            public void HandleNewSkillLogLine(string line)
            {
                if (line.StartsWith("Favor increased"))
                {
                    float level = GeneralHelper.ExtractSkillLEVELFromLine(line);
                    if (level > -1)
                    {
                        CurrentFavorLevel = level;
                    }
                }
            }

            public void Update()
            {
                FavorHandler.Update();
            }
        }
    }
}
