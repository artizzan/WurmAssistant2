using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using Aldurcraft.WurmOnline.WurmLogsManager;
using System.Text.RegularExpressions;
using Aldurcraft.Utility;
using Aldurcraft.Utility.SoundEngine;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.SoundNotify
{
    public class SoundNotifier
    {
        [DataContract]
        public class NotifierSettings
        {
            [DataMember]
            public bool Muted = false;
            [DataMember]
            public double QueueDefDelay = 1.0D;
            [DataMember]
            public bool QueueSoundEnabled = false;
            [DataMember]
            public string QueueSoundName = null;
        }

        UControlSoundNotifyPlayerController controlUI;
        ModuleSoundNotify ParentModule;
        public PersistentObject<NotifierSettings> Settings;
        public string Player;

        string thisNotifierDataDir;

        public SoundNotifier(ModuleSoundNotify parentModule, string player, string moduleDataDir)
        {
            this.ParentModule = parentModule;
            Player = player;
            thisNotifierDataDir = Path.Combine(moduleDataDir, player);
            if (!Directory.Exists(thisNotifierDataDir)) Directory.CreateDirectory(thisNotifierDataDir);

            Settings = new PersistentObject<NotifierSettings>(new NotifierSettings());
            Settings.FilePath = Path.Combine(thisNotifierDataDir, "settings.xml");
            if (!Settings.Load())
            {
                Settings.Save();
            }

            //create control for Module UI
            controlUI = new UControlSoundNotifyPlayerController();

            //create this notifier UI
            SoundManagerUI = new FormSoundNotifyConfig(this);

            UpdateMutedState();
            controlUI.label1.Text = player;
            controlUI.buttonMute.Click += ToggleMute;
            controlUI.buttonConfigure.Click += Configure;
            controlUI.buttonRemove.Click += Stop;

            InitPortedCode(Player);
            WurmLogs.SubscribeToLogFeed(this.Player, OnNewLogEvents);
        }

        public UControlSoundNotifyPlayerController GetUIHandle()
        {
            return controlUI;
        }

        private void ToggleMute(object sender, EventArgs e)
        {
            Settings.Value.Muted = !Settings.Value.Muted;
            Settings.DelayedSave();
            UpdateMutedState();
            SoundManagerUI.UpdateMutedState();
        }

        private bool Muted
        {
            get { return Settings.Value.Muted || ParentModule.Settings.Value.GlobalMute; }
        }

        public void UpdateMutedState()
        {
            if (Settings.Value.Muted) controlUI.buttonMute.BackgroundImage = Properties.Resources.SoundDisabledSmall;
            else controlUI.buttonMute.BackgroundImage = Properties.Resources.SoundEnabledSmall;
        }

        ////////////////// 

        public void Update(bool engineInSleepMode)
        {
            Settings.Update();
            UpdateQueueSound(engineInSleepMode);
        }

        private void Configure(object sender, EventArgs e)
        {
            //open soundnotify form
            ToggleUI();
        }

        public void Stop(object sender, EventArgs e)
        {
            Settings.Save();
            WurmLogs.UnsubscribeFromLogFeed(this.Player, OnNewLogEvents);
            ParentModule.RemoveNotifier(this);

            controlUI.Dispose();
            SoundManagerUI.Close();
        }

        public void OnNewLogEvents(object sender, NewLogEntriesEventArgs e)
        {
            if (e.Entries.PlayerName == this.Player)
            {
                foreach (var entry in e.Entries.AllEntries)
                {
                    HandleNewLogEvents(entry.Entries, entry.LogType);
                }
            }
        }

        ////////////////// ported stuff from WA 1.x ModuleSoundNotify

        FormSoundNotifyConfig SoundManagerUI;
        List<PlaylistEntry> Playlist = new List<PlaylistEntry>();

        // playlists cached per log type
        List<PlaylistEntryCacheable> EventPlaylist = new List<PlaylistEntryCacheable>();
        List<PlaylistEntryCacheable> CombatPlaylist = new List<PlaylistEntryCacheable>();
        List<PlaylistEntryCacheable> AlliancePlaylist = new List<PlaylistEntryCacheable>();
        List<PlaylistEntryCacheable> CA_HELPPlaylist = new List<PlaylistEntryCacheable>();
        List<PlaylistEntryCacheable> FreedomPlaylist = new List<PlaylistEntryCacheable>();
        List<PlaylistEntryCacheable> FriendsPlaylist = new List<PlaylistEntryCacheable>();
        List<PlaylistEntryCacheable> GLFreedomPlaylist = new List<PlaylistEntryCacheable>();
        List<PlaylistEntryCacheable> LocalPlaylist = new List<PlaylistEntryCacheable>();
        List<PlaylistEntryCacheable> MGMTPlaylist = new List<PlaylistEntryCacheable>();
        List<PlaylistEntryCacheable> SkillsPlaylist = new List<PlaylistEntryCacheable>();
        List<PlaylistEntryCacheable> TeamPlaylist = new List<PlaylistEntryCacheable>();
        List<PlaylistEntryCacheable> VillagePlaylist = new List<PlaylistEntryCacheable>();
        List<PlaylistEntryCacheable> PMPlaylist = new List<PlaylistEntryCacheable>();

        // last queue ending action
        DateTime lastActionFinished = DateTime.Now;
        // last queue starting action
        DateTime lastActionStarted = DateTime.Now;

        // whether queue sound is scheduled to play on next update
        bool scheduledQueueSound = false;
        // wurm log line that triggered queue sound
        string LogEntryThatTriggeredLastQueueSound;

        // used if no custom sound set
        static SB_SoundPlayer defQueueSoundPlayer;
        bool defQueueSoundPlayerEnabled = true;

        // previous processed line
        string lastline;
        private string lastEventLine = string.Empty;
        private bool levelingMode = false;

        TextFileObject PlaylistTextFile;
        public readonly Char[] DefDelimiter = new Char[] { ';' };


        void InitPortedCode(string playername)
        {
            PlaylistTextFile = new TextFileObject(Path.Combine(thisNotifierDataDir, "playlist.txt"), true, false, true, false, false, false);
            LoadPlaylist();

            try
            {
                defQueueSoundPlayer = new SB_SoundPlayer(Path.Combine(ParentModule.ModuleAssetDir, "defQueueSound.ogg"));
                defQueueSoundPlayer.Load(volumeAdjust: false);
            }
            catch (Exception _e)
            {
                Logger.LogError("could not load default queue sound", this, _e);
                defQueueSoundPlayerEnabled = false;
            }
        }

        public void ToggleUI()
        {
            if (SoundManagerUI.Visible) SoundManagerUI.RestoreFromMin();
            else { SoundManagerUI.Show(); }
        }

        #region PLAYLIST

        void LoadPlaylist()
        {
            //check file version
            bool queueSoundFixNeeded = false;
            string version = PlaylistTextFile.ReadNextLine();

            int activePos;
            int soundnamePos;
            int conditionPos;
            int specialPos;
            // assign index positions for data in file in respect to version
            if (version == "FILEVERSION 3")
            {
                activePos = 0;
                soundnamePos = 1;
                conditionPos = 2;
                specialPos = 3;
            }
            else if (version == "FILEVERSION 2")
            {
                activePos = 0;
                soundnamePos = 1;
                conditionPos = 2;
                specialPos = 3;
                queueSoundFixNeeded = true;
            }
            else //old versionless playlist
            {
                PlaylistTextFile.resetReadPos(); //it has no version line
                activePos = -1; //def indicator that it doesnt exist in this version
                soundnamePos = 0;
                conditionPos = 1;
                specialPos = 2;
            }

            string line = PlaylistTextFile.ReadNextLine();
            while (line != null)
            {
                PlaylistEntry playlistentry = new PlaylistEntry();

                //parse line entries
                string[] entries = line.Split(DefDelimiter);
                if (activePos != -1)
                {
                    if (Convert.ToBoolean(entries[activePos]) == false) playlistentry.isActive = false;
                    else playlistentry.isActive = true;
                }
                playlistentry.SoundName = entries[soundnamePos];
                playlistentry.Soundplayer = SoundBank.GetSoundPlayer(playlistentry.SoundName);
                playlistentry.Condition = entries[conditionPos];
                for (int i = specialPos; i < entries.Length; i++)
                {
                    playlistentry.SpecialSettings.Add(entries[i]);
                    if (entries[i].Contains("s:CustomRegex")) playlistentry.isCustomRegex = true;
                }
                Playlist.Add(playlistentry);
                line = PlaylistTextFile.ReadNextLine();
            }

            if (queueSoundFixNeeded) MoveQueueSound();
            CacheSpecializedPlaylists();
        }

        private void MoveQueueSound()
        {
            int plistEntryToRemove = -1;
            for (int i = 0; i < Playlist.Count; i++)
            {
                foreach (string specialcond in Playlist[i].SpecialSettings)
                {
                    if (specialcond == "s:queue sound")
                    {
                        Settings.Value.QueueSoundName = Playlist[i].SoundName;
                        plistEntryToRemove = i;
                        //handle moving special sound to the new setting
                    }
                }
            }
            if (plistEntryToRemove > -1)
            {
                Playlist.RemoveAt(plistEntryToRemove);
            }
            SavePlaylist();
        }

        private void CacheSpecializedPlaylists()
        {
            EventPlaylist.Clear();
            CombatPlaylist.Clear();
            AlliancePlaylist.Clear();
            CA_HELPPlaylist.Clear();
            FreedomPlaylist.Clear();
            FriendsPlaylist.Clear();
            GLFreedomPlaylist.Clear();
            LocalPlaylist.Clear();
            MGMTPlaylist.Clear();
            SkillsPlaylist.Clear();
            TeamPlaylist.Clear();
            VillagePlaylist.Clear();
            PMPlaylist.Clear();

            foreach (PlaylistEntry entry in Playlist)
            {
                foreach (string cond in entry.SpecialSettings)
                {
                    if (cond == GameLogTypesEX.GetNameForLogType(GameLogTypes.Event))
                    {
                        EventPlaylist.Add(new PlaylistEntryCacheable(entry.Soundplayer, entry.Condition, entry.SoundName, entry.isActive));
                    }
                    if (cond == GameLogTypesEX.GetNameForLogType(GameLogTypes.Combat))
                    {
                        CombatPlaylist.Add(new PlaylistEntryCacheable(entry.Soundplayer, entry.Condition, entry.SoundName, entry.isActive));
                    }
                    if (cond == GameLogTypesEX.GetNameForLogType(GameLogTypes.Alliance))
                    {
                        AlliancePlaylist.Add(new PlaylistEntryCacheable(entry.Soundplayer, entry.Condition, entry.SoundName, entry.isActive));
                    }
                    if (cond == GameLogTypesEX.GetNameForLogType(GameLogTypes.CA_HELP))
                    {
                        CA_HELPPlaylist.Add(new PlaylistEntryCacheable(entry.Soundplayer, entry.Condition, entry.SoundName, entry.isActive));
                    }
                    if (cond == GameLogTypesEX.GetNameForLogType(GameLogTypes.Freedom))
                    {
                        FreedomPlaylist.Add(new PlaylistEntryCacheable(entry.Soundplayer, entry.Condition, entry.SoundName, entry.isActive));
                    }
                    if (cond == GameLogTypesEX.GetNameForLogType(GameLogTypes.Friends))
                    {
                        FriendsPlaylist.Add(new PlaylistEntryCacheable(entry.Soundplayer, entry.Condition, entry.SoundName, entry.isActive));
                    }
                    if (cond == GameLogTypesEX.GetNameForLogType(GameLogTypes.GLFreedom))
                    {
                        GLFreedomPlaylist.Add(new PlaylistEntryCacheable(entry.Soundplayer, entry.Condition, entry.SoundName, entry.isActive));
                    }
                    if (cond == GameLogTypesEX.GetNameForLogType(GameLogTypes.Local))
                    {
                        LocalPlaylist.Add(new PlaylistEntryCacheable(entry.Soundplayer, entry.Condition, entry.SoundName, entry.isActive));
                    }
                    if (cond == GameLogTypesEX.GetNameForLogType(GameLogTypes.MGMT))
                    {
                        MGMTPlaylist.Add(new PlaylistEntryCacheable(entry.Soundplayer, entry.Condition, entry.SoundName, entry.isActive));
                    }
                    if (cond == GameLogTypesEX.GetNameForLogType(GameLogTypes.Skills))
                    {
                        SkillsPlaylist.Add(new PlaylistEntryCacheable(entry.Soundplayer, entry.Condition, entry.SoundName, entry.isActive));
                    }
                    if (cond == GameLogTypesEX.GetNameForLogType(GameLogTypes.Team))
                    {
                        TeamPlaylist.Add(new PlaylistEntryCacheable(entry.Soundplayer, entry.Condition, entry.SoundName, entry.isActive));
                    }
                    if (cond == GameLogTypesEX.GetNameForLogType(GameLogTypes.Village))
                    {
                        VillagePlaylist.Add(new PlaylistEntryCacheable(entry.Soundplayer, entry.Condition, entry.SoundName, entry.isActive));
                    }
                    if (cond == GameLogTypesEX.GetNameForLogType(GameLogTypes.PM))
                    {
                        PMPlaylist.Add(new PlaylistEntryCacheable(entry.Soundplayer, entry.Condition, entry.SoundName, entry.isActive));
                    }
                }
            }
        }

        /// <summary>
        /// called from GUI Form
        /// </summary>
        /// <param name="soundname"></param>
        /// <param name="cond"></param>
        /// <param name="speccond"></param>
        /// <param name="insertIndex">needed only for inserting, def -1 for adding</param>
        public void AddPlaylistEntry(string soundname, string cond, List<string> speccond, bool active, int insertIndex = -1)
        {
            PlaylistEntry playlistentry = new PlaylistEntry();
            playlistentry.SoundName = soundname;
            playlistentry.Soundplayer = SoundBank.GetSoundPlayer(playlistentry.SoundName);
            playlistentry.Condition = cond;
            if (speccond != null)
            {
                foreach (string _cond in speccond)
                {
                    playlistentry.SpecialSettings.Add(_cond);
                    if (_cond.Contains("s:CustomRegex")) playlistentry.isCustomRegex = true;
                }
            }
            if (insertIndex == -1) Playlist.Add(playlistentry);
            else Playlist.Insert(insertIndex, playlistentry);
            SavePlaylist();
        }

        // called from GUI Form
        public void RemovePlaylistEntry(int index)
        {
            Playlist.RemoveAt(index);
            SavePlaylist();
        }

        public List<PlaylistEntry> getPlaylist()
        {
            return Playlist;
        }

        public PlaylistEntry getPlaylistEntryAtIndex(int parIndex)
        {
            try { return Playlist[parIndex]; }
            catch { return null; }
        }

        public void TogglePlaylistEntryActive(int index)
        {
            if (Playlist[index].isActive) Playlist[index].isActive = false;
            else Playlist[index].isActive = true;
            SavePlaylist();
        }

        void SavePlaylist()
        {
            //backup old file format
            try
            {
                string PLversion = PlaylistTextFile.ReadLine(0);
                if (PLversion != null)
                {
                    if (!PLversion.StartsWith("FILEVERSION 3", StringComparison.Ordinal))
                    {
                        Logger.LogInfo("SoundNotify: detected old playlist format, attempting to backup before conversion");

                        try
                        {
                            PlaylistTextFile.BackupFile();
                            Logger.LogInfo("SoundNotify: backup successful");
                        }
                        catch (Exception _e)
                        {
                            Logger.LogInfo("SoundNotify: Exception while backing up, may have failed", null, _e);
                        }
                    }
                }
            }
            catch (Exception _e)
            {
                Logger.LogError("! Uknown exception", this, _e);
                PlaylistTextFile.ClearFile();
            }

            PlaylistTextFile.ClearFile();

            //write this file version
            PlaylistTextFile.WriteLine("FILEVERSION 3");

            foreach (PlaylistEntry entry in Playlist)
            {
                string DBrecord = entry.isActive.ToString();
                DBrecord += DefDelimiter[0];
                DBrecord += entry.SoundName;
                DBrecord += DefDelimiter[0];
                DBrecord += entry.Condition;
                foreach (string specialcond in entry.SpecialSettings)
                {
                    DBrecord += DefDelimiter[0];
                    DBrecord += specialcond;
                }
                PlaylistTextFile.WriteLine(DBrecord);
            }
            //MoveQueueSound();
            CacheSpecializedPlaylists();
        }

        public string ConvertCondOutputToRegex(string cond)
        {
            cond = Regex.Escape(cond);
            cond = cond.Replace(@"\*", @".+");
            return cond;
        }

        public string ConvertRegexToCondOutput(string regexCond)
        {
            regexCond = regexCond.Replace(@".+", @"\*");
            regexCond = Regex.Unescape(regexCond);
            return regexCond;
        }

        #endregion

        public float GetSoundEngineVolume()
        {
            return SoundBank.GlobalVolume;
        }

        //button click in ui
        public void SetQueueSound()
        {
            FormChooseSound ChooseSoundUI = new FormChooseSound();
            if (ChooseSoundUI.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Settings.Value.QueueSoundName = ChooseSoundUI.ChosenSound;
                SoundManagerUI.UpdateSoundName(Settings.Value.QueueSoundName);
                Settings.DelayedSave();
            }
        }

        //get the name for ui
        public string GetQueueSoundForUI()
        {
            if (Settings.Value.QueueSoundName != null) return Settings.Value.QueueSoundName;
            else return "default";
        }

        #region MODULE ENGINE

        //TODO modify to be handled with new wurm logs manager
        private void HandleNewLogEvents(List<string> newLogEvents, GameLogTypes logType)
        {
            List<PlaylistEntryCacheable> currentPlaylist;

            switch (logType)
            {
                case GameLogTypes.Event:
                    currentPlaylist = EventPlaylist;
                    break;
                case GameLogTypes.Combat:
                    currentPlaylist = CombatPlaylist;
                    break;
                case GameLogTypes.Alliance:
                    currentPlaylist = AlliancePlaylist;
                    break;
                case GameLogTypes.CA_HELP:
                    currentPlaylist = CA_HELPPlaylist;
                    break;
                case GameLogTypes.Freedom:
                    currentPlaylist = FreedomPlaylist;
                    break;
                case GameLogTypes.Friends:
                    currentPlaylist = FriendsPlaylist;
                    break;
                case GameLogTypes.GLFreedom:
                    currentPlaylist = GLFreedomPlaylist;
                    break;
                case GameLogTypes.Local:
                    currentPlaylist = LocalPlaylist;
                    break;
                case GameLogTypes.MGMT:
                    currentPlaylist = MGMTPlaylist;
                    break;
                case GameLogTypes.Skills:
                    currentPlaylist = SkillsPlaylist;
                    break;
                case GameLogTypes.Team:
                    currentPlaylist = TeamPlaylist;
                    break;
                case GameLogTypes.Village:
                    currentPlaylist = VillagePlaylist;
                    break;
                case GameLogTypes.PM:
                    currentPlaylist = PMPlaylist;
                    break;
                default: throw new Exception("No cached playlist for this log type: " + logType);
            }

            lastline = "";
            foreach (string line in newLogEvents)
            {
                if (lastline != line)
                {
                    Logger.LogDebug("> SoundNotify > HandleNewLogEvents > line processed");
                    // determine if queue sound should be played
                    if (Settings.Value.QueueSoundEnabled && logType == GameLogTypes.Event)
                        handleQueueSound(line);

                    if (!Muted)
                    {
                        foreach (PlaylistEntryCacheable playlistentry in currentPlaylist)
                        {
                            if (playlistentry.isActive
                                && playlistentry.Soundplayer != null
                                && playlistentry.Condition != ""
                                && Regex.IsMatch(line, playlistentry.Condition, RegexOptions.IgnoreCase))
                            {
                                try
                                {
                                    playlistentry.Soundplayer.Play();
                                    Logger.LogInfo("Sound notify played sound: " + playlistentry.SoundName + " on event: " + line);
                                }
                                catch (FileNotFoundException _e)
                                {
                                    Logger.LogInfo("Sound notify could not play sound " + playlistentry.SoundName + ", reason: " + _e.Message);
                                }

                            }
                        }
                    }
                }
                lastline = line;
            }
        }

        private void handleQueueSound(string line)
        {
            bool _PlayerActionStarted = false;

            foreach (string cond in LogQueueParseHelper.ActionStart)
            {
                if (line.StartsWith(cond, StringComparison.Ordinal))
                {
                    _PlayerActionStarted = true;
                    levelingMode = false;
                }
            }
            foreach (string cond in LogQueueParseHelper.ActionStart_contains)
            {
                if (line.Contains(cond))
                {
                    _PlayerActionStarted = true;
                    levelingMode = false;
                }
            }

            if (levelingMode)
            {
                foreach (string cond in LogQueueParseHelper.LevelingEnd)
                {
                    if (line.StartsWith(cond, StringComparison.Ordinal))
                    {
                        levelingMode = false;
                    }
                }
            }
            if (line.StartsWith(LogQueueParseHelper.LevelingModeStart, StringComparison.Ordinal))
            {
                levelingMode = true;
            }

            foreach (string cond in LogQueueParseHelper.ActionFalstart)
            {
                if (line.StartsWith(cond, StringComparison.Ordinal)) _PlayerActionStarted = false;
            }
            if (_PlayerActionStarted == true)
            {
                lastActionStarted = DateTime.Now;
            }

            bool _PlayerActionFinished = false;

            foreach (string cond in LogQueueParseHelper.ActionEnd)
            {
                if (line.StartsWith(cond, StringComparison.Ordinal)) _PlayerActionFinished = true;
            }
            foreach (string cond in LogQueueParseHelper.ActionEnd_contains)
            {
                if (line.Contains(cond))
                    _PlayerActionFinished = true;
            }
            foreach (string cond in LogQueueParseHelper.ActionFalsEnd)
            {
                if (line.StartsWith(cond, StringComparison.Ordinal)) 
                    _PlayerActionFinished = false;
            }
            foreach (string cond in LogQueueParseHelper.ActionFalsEndDueToLastAction)
            {
                if (lastEventLine.StartsWith(cond, StringComparison.Ordinal)) 
                    _PlayerActionFinished = false;
            }

            if (levelingMode) _PlayerActionFinished = false;
            if (_PlayerActionFinished == true)
            {
                LogEntryThatTriggeredLastQueueSound = line;
                lastActionFinished = DateTime.Now;
                // if action finished, older action started is no longer valid
                // and should not disable queuesound in next conditional
                lastActionStarted = lastActionStarted.AddSeconds(-Settings.Value.QueueDefDelay); // datetime is not nullable
                scheduledQueueSound = true;
            }

            // disable scheduled queue sound if new action started before its played
            if (lastActionStarted.AddSeconds(Settings.Value.QueueDefDelay) >= DateTime.Now)
            {
                scheduledQueueSound = false;
            }

            lastEventLine = line;
        }

        //TODO call this in update method
        void UpdateQueueSound(bool engineInSleepMode)
        {
            if (scheduledQueueSound
                && DateTime.Now >= lastActionFinished.AddSeconds(Settings.Value.QueueDefDelay))
            {
                if (!Muted)
                {
                    if (Settings.Value.QueueSoundName != null)
                    {
                        SoundBank.PlaySound(Settings.Value.QueueSoundName);
                        Logger.LogInfo("Sound notify played queue sound due to event: " + LogEntryThatTriggeredLastQueueSound);
                    }
                    else
                    {
                        defQueueSoundPlayer.Play();
                        Logger.LogInfo("Sound notify played default queue sound due to event: " + LogEntryThatTriggeredLastQueueSound);
                    }
                }
                scheduledQueueSound = false;
            }
        }

        #endregion
    }
}
