using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Aldurcraft.Utility;
using Aldurcraft.WurmOnline.WurmState;

namespace Aldurcraft.WurmOnline.WurmLogsManager
{
    public class LogEngine : IDisposable
    {
        GameLogState EventLogState;
        GameLogState CombatLogState;
        GameLogState AllianceLogState;
        GameLogState CA_HELPLogState;
        GameLogState FreedomLogState;
        GameLogState FriendsLogState;
        GameLogState GLFreedomLogState;
        GameLogState LocalLogState;
        GameLogState MGMTLogState;
        GameLogState SkillsLogState;
        GameLogState TeamLogState;
        GameLogState VillageLogState;

        string PlayerName;
        string WurmLogsDir;
        bool IsDailyLogging;

        List<GameLogState> GenericLogsList = new List<GameLogState>();
        List<GameLogState> PMLogsList = new List<GameLogState>();
        List<GameLogState> CombinedLogsList = new List<GameLogState>();

        int lastLogCount = 0;

        internal LogEngine(string playerName, bool dailyLoggingMode)
        {
            this.IsDailyLogging = dailyLoggingMode;
            this.PlayerName = playerName;
            this.WurmLogsDir = WurmClient.WurmPaths.GetLogsDirForPlayer(playerName);
            if (WurmLogsDir == null) throw new Exception("!!! Failed to start LogsManager for: " + PlayerName);
            else Initialize();
        }

        void Initialize()
        {
            Logger.LogInfo("> Initializing wrappers for Wurm log files ");
            // init all the log wrappers except PM and add them to Generic list
            EventLogState = new GameLogState(WurmLogsDir, GameLogTypes.Event, IsDailyLogging);
            GenericLogsList.Add(EventLogState);
            CombatLogState = new GameLogState(WurmLogsDir, GameLogTypes.Combat, IsDailyLogging);
            GenericLogsList.Add(CombatLogState);
            AllianceLogState = new GameLogState(WurmLogsDir, GameLogTypes.Alliance, IsDailyLogging);
            GenericLogsList.Add(AllianceLogState);
            CA_HELPLogState = new GameLogState(WurmLogsDir, GameLogTypes.CA_HELP, IsDailyLogging);
            GenericLogsList.Add(CA_HELPLogState);
            FreedomLogState = new GameLogState(WurmLogsDir, GameLogTypes.Freedom, IsDailyLogging);
            GenericLogsList.Add(FreedomLogState);
            FriendsLogState = new GameLogState(WurmLogsDir, GameLogTypes.Friends, IsDailyLogging);
            GenericLogsList.Add(FriendsLogState);
            GLFreedomLogState = new GameLogState(WurmLogsDir, GameLogTypes.GLFreedom, IsDailyLogging);
            GenericLogsList.Add(GLFreedomLogState);
            LocalLogState = new GameLogState(WurmLogsDir, GameLogTypes.Local, IsDailyLogging);
            GenericLogsList.Add(LocalLogState);
            MGMTLogState = new GameLogState(WurmLogsDir, GameLogTypes.MGMT, IsDailyLogging);
            GenericLogsList.Add(MGMTLogState);
            SkillsLogState = new GameLogState(WurmLogsDir, GameLogTypes.Skills, IsDailyLogging);
            GenericLogsList.Add(SkillsLogState);
            TeamLogState = new GameLogState(WurmLogsDir, GameLogTypes.Team, IsDailyLogging);
            GenericLogsList.Add(TeamLogState);
            VillageLogState = new GameLogState(WurmLogsDir, GameLogTypes.Village, IsDailyLogging);
            GenericLogsList.Add(VillageLogState);

            // init all PM logs and add them to PM list, then combine Generic and PM lists to Combined list
            ManagePMLogs();

            int numOfLogFilesFound = 0;
            foreach (var log in CombinedLogsList)
            {
                if (log.LogTextFileExists) numOfLogFilesFound++;
            }
            if (numOfLogFilesFound == 0)
            {
                Logger.LogInfo("? No log files acquired for " + PlayerName + ", path: " + WurmLogsDir, this);
            }
            else
            {
                Logger.LogInfo("> Tracking " + numOfLogFilesFound + " logs for " + PlayerName);
            }
        }

        internal void ManagePMLogs()
        {
            string[] AllLogFiles = Directory.GetFiles(WurmLogsDir);
            if (AllLogFiles.Length != lastLogCount)
            {
                PMLogsList.Clear();
                foreach (string file in AllLogFiles)
                {
                    string workstring = Path.GetFileNameWithoutExtension(file);
                    if (workstring.StartsWith("PM", StringComparison.Ordinal))
                    {
                        //filename parsing also in gamelogstate
                        if (workstring.EndsWith(DateTime.Now.ToString("yyyy-MM-dd")))
                        {
                            string playername = workstring.Remove(0, 4);
                            playername = playername.Remove(playername.IndexOf('.'));
                            GameLogState newlog = new GameLogState(WurmLogsDir, GameLogTypes.PM, IsDailyLogging, playername);
                            PMLogsList.Add(newlog);
                        }
                        else if (workstring.EndsWith(DateTime.Now.ToString("yyyy-MM")))
                        {
                            string playername = workstring.Remove(0, 4);
                            playername = playername.Remove(playername.IndexOf('.'));
                            GameLogState newlog = new GameLogState(WurmLogsDir, GameLogTypes.PM, IsDailyLogging, playername);
                            PMLogsList.Add(newlog);
                        }
                    }
                }
                CombinedLogsList.Clear();
                CombinedLogsList.AddRange(GenericLogsList);
                CombinedLogsList.AddRange(PMLogsList);
                lastLogCount = AllLogFiles.Length;
                UpdateIfDisplayLogEntries();
            }
        }

        internal void UpdateIfDisplayLogEntries()
        {
            foreach (var log in CombinedLogsList)
            {
                //TODO
                //log.displayEvents = WurmAssistant.ZeroRef.DisplayAllLogEvents;
            }
        }

        internal NewLogEntries GetNewEvents()
        {
            NewLogEntries result = new NewLogEntries();
            foreach (GameLogState log in CombinedLogsList)
            {
                List<string> newentries = log.UpdateAndGetNewEvents();

                if (newentries != null)
                {
                    NewLogEntriesContainer logentries = new NewLogEntriesContainer();
                    logentries.LogType = log.LogType;
                    if (log.LogType == GameLogTypes.PM) logentries.PM_Player = log.PM_Name;
                    logentries.EntriesWithTimestamps = newentries;
                    logentries.Entries = RemoveTimestamps_DeepCopy(newentries);
                    result.AllEntries.Add(logentries);
                }
            }
            return result;
        }

        internal void UpdateAndBroadcastNewEvents()
        {
            bool _refreshingDTandU = false;
            //TODO opti: avoid doing updates if no one subscribed
            NewLogEntries entries = GetNewEvents();
            if (!entries.IsEmpty)
            {
                entries.PlayerName = this.PlayerName;
                OnNewLogEntries(new NewLogEntriesEventArgs(entries));
                foreach (var entry in entries.AllEntries)
                {
                    if (entry.LogType == GameLogTypes.Event)
                    {
                        foreach (var line in entry.Entries)
                        {
                            // this is done to trigger webfeed refresh
                            if (!_refreshingDTandU && line.Contains("You are on"))
                            {
                                _refreshingDTandU = true;
                                //don't await, wurm server will handle everything afterwards
                                WurmState.WurmServer.RefreshDateTimeAndUptime();
                            }

                            WurmState.PlayerServerTracker.ProcessLogEvent(line, PlayerName);
                            WurmState.WurmServer.HandleNewLogEvent(line, PlayerName);
                        }
                    }
                }
            }
        }

        public event EventHandler<NewLogEntriesEventArgs> NewLogEntries;

        protected virtual void OnNewLogEntries(NewLogEntriesEventArgs e)
        {
            var eh = NewLogEntries;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        List<string> RemoveTimestamps_DeepCopy(List<string> stampedEntries)
        {
            List<string> result = new List<string>();
            foreach (string line in stampedEntries)
            {
                try
                {
                    result.Add(line.Remove(0, 11));
                }
                catch (Exception _e)
                {
                    Logger.LogDebug("error at cleartimestamps: " + line, this, _e);
                }
            }
            return result;
        }

        public void Dispose()
        {
            //clear event handlers to allow GC
            NewLogEntries = null;
        }
    }

    public class NewLogEntries
    {
        /// <summary>
        /// Name of the wurm character, that these entries come from
        /// </summary>
        public string PlayerName;

        /// <summary>
        /// List of all new wurm log messages since last update
        /// </summary>
        public List<NewLogEntriesContainer> AllEntries = new List<NewLogEntriesContainer>();

        internal bool IsEmpty
        {
            get { return AllEntries.Count == 0; }
        }
    }

    public struct NewLogEntriesContainer
    {
        /// <summary>
        /// Type of the log, that these messages come from
        /// </summary>
        public GameLogTypes LogType;
        /// <summary>
        /// Shows PM recipient character name for GameLogTypes.PM, in other cases is null
        /// </summary>
        public string PM_Player;

        /// <summary>
        /// List of wurm log messages, with their timestamp [hh:mm:ss] and following whitespace removed for easier parsing
        /// treat as read-only
        /// </summary>
        public List<string> Entries;

        /// <summary>
        /// List of RAW wurm log messages (including their timestamps),
        /// treat as read-only
        /// </summary>
        public List<string> EntriesWithTimestamps;
    }

    public class NewLogEntriesEventArgs : EventArgs
    {
        /// <summary>
        /// treat as read-only
        /// </summary>
        public readonly NewLogEntries Entries;

        public NewLogEntriesEventArgs(NewLogEntries entries)
        {
            this.Entries = entries;
        }
    }

}
