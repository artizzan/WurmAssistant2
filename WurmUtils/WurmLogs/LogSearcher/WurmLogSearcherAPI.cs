using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Aldurcraft.Utility;
using Aldurcraft.Utility.WurmHelpers;

namespace Aldurcraft.WurmOnline.WurmLogsManager.Searcher
{
    using ServerInfo = WurmState.WurmServer.ServerInfo;
    /// <summary>
    /// Provides means to search wurm game logs
    /// </summary>
    public static class WurmLogSearcherAPI
    {
        const string THIS = "WurmLogSearcherAPI";

        static LogSearchManager LogSearchMan;
        static FormLogSearcher SearcherUI;

        static bool isInitialized = false;

        /// <summary>
        /// Initialize LogSearcher with optional directory path, where cache database will be stored.
        /// </summary>
        /// <param name="dirPath">Absolute directory path, default null will use CodeBase</param>
        /// <param name="wipeExistingDb">Clears any existing database at dirPath location</param>
        /// <exception cref="InvalidOperationException">LogSearcher was already initialized</exception>
        /// <exception cref="Exception">There was an error while trying to initialize</exception>
        /// <returns></returns>
        public static void Initialize(string dirPath = null, bool wipeExistingDb = false)
        {
            if (isInitialized) throw new InvalidOperationException("LogSearcher already initialized");

            try
            {
                Logger.LogInfo("Initializing API", THIS);
                LogSearchMan = new LogSearchManager();
                LogSearchMan.CreateCacheDB(dirPath, wipeExistingDb); //wipeExistingDb); //exc here stop the init and allow recovery
                LogSearchMan.Initialize();
                LogSearchMan.UpdateCache(); //ensure updates before first search
                SearcherUI = new FormLogSearcher(LogSearchMan);
                SearcherUI.WindowState = System.Windows.Forms.FormWindowState.Minimized;
                SearcherUI.Show(); //need to create window handle
                SearcherUI.Hide();
                SearcherUI.StartUpdateLoop();
                Logger.LogInfo("Init completed", THIS);
                isInitialized = true;
                _tcs.SetResult(true);
            }
            catch (Exception _e)
            {
                Logger.LogError("API init error", THIS, _e);
                throw;
            }
        }

        private static TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();
        public static async Task AwaitInitializationAsync()
        {
            if (isInitialized) return;
            await _tcs.Task;
        }

        /// <summary>
        /// Show or hide log searcher GUI
        /// </summary>
        /// <exception cref="InvalidOperationException">LogSearcher was not initialized</exception>
        public static void ToggleUI()
        {
            if (!isInitialized) ThrowInitException();

            SearcherUI.ShowThisDarnWindowDammitEx();
        }

        /// <summary>
        /// Performs search through Wurm logs with the supplied search data.
        /// Returns null if search fails, reason is logged.
        /// </summary>
        /// <param name="logsearchdata">requires search criteria, 
        /// sending object without it will result in exception</param>
        /// <exception cref="ArgumentNullException">supplied LogSearchData had no search criteria</exception>
        /// <exception cref="InvalidOperationException">LogSearcher was not initialized</exception>
        /// <returns></returns>
        public static async Task<LogSearchData> SearchWurmLogsAsync(LogSearchData logsearchdata)
        {
            if (!isInitialized) ThrowInitException();

            Logger.LogDebug("Enqueuing custom search", THIS);
            return await LogSearchMan.PerformSearchAsync(logsearchdata);
        }

        static void ThrowInitException()
        {
            throw new InvalidOperationException(THIS + " not initialized");
        }

        /// <summary>
        /// Performs search through Wurm logs with the supplied search data. 
        /// Results are filtered by server group and log entries from other groups are discarded.
        /// Returns null if search fails, reason is logged.
        /// </summary>
        /// <param name="logsearchdata">requires search criteria, 
        /// sending object without it will result in exception</param>
        /// <exception cref="ArgumentNullException">supplied LogSearchData had no search criteria</exception>
        /// <exception cref="InvalidOperationException">LogSearcher was not initialized</exception>
        /// <param name="group">Server group type</param>
        /// <returns></returns>
        public static async Task<LogSearchData> SearchWurmLogsFilteredByServerGroupAsync(LogSearchData logsearchdata, ServerInfo.ServerGroup group)
        {
            // for other logs than event, there is no "You are on", 
            // this needs a secondary mirror search through events,
            // build cache of contraits to verify if line should be discarded
            // FIXED


            // TODO create proper cache?

            ServerGroupTimeTable timetable = null;
            if (logsearchdata.SearchCriteria.GameLogType != GameLogTypes.Event)
            {
                string playername = logsearchdata.SearchCriteria.Player;
                int daysToLookBack = (int)(DateTime.Now - logsearchdata.SearchCriteria.TimeFrom).TotalDays + 1;
                timetable = await CreateSGTimeTable(playername, daysToLookBack);
            }

            logsearchdata = await SearchWurmLogsAsync(logsearchdata);

            object[] args = new object[2] { logsearchdata, timetable };

            Task<LogSearchData> task = new Task<LogSearchData>(x =>
                {
                    try
                    {
                        var lgs = (LogSearchData)args[0];
                        var ttable = args[1] == null ? null : (ServerGroupTimeTable)args[1];
                        List<string> newResults = new List<string>();
                        bool validServerGroup = false;
                        foreach (string line in lgs.AllLines)
                        {
                            //check if valid server group
                            if (lgs.SearchCriteria.GameLogType == GameLogTypes.Event && line.Contains("You are on"))
                            {
                                string server;
                                ServerInfo.ServerGroup? extractedGroup = TryGetServerGroupFromLine(line, out server);
                                if (extractedGroup == group) validServerGroup = true;
                                else validServerGroup = false;
                            }
                            else if (lgs.SearchCriteria.GameLogType != GameLogTypes.Event)
                            {
                                ServerInfo.ServerGroup extractedGroup = ttable.GetGroupForDate(LogSearchManager.BuildDateForMatch(line));
                                if (extractedGroup == group) validServerGroup = true;
                                else validServerGroup = false;
                            }
                            if (validServerGroup)
                            {
                                newResults.Add(line);
                            }
                        }
                        lgs.AllLines = newResults;
                        return lgs;
                    }
                    catch (Exception _e)
                    {
                        Logger.LogCritical("problem while performing group-filtered log search", THIS, _e);
                        return null;
                    }
                }, args);
            task.Start();

            return await task;
        }

        internal class ServerGroupTimeTable
        {
            class GroupData
            {
                public DateTime DateFrom;
                public DateTime DateTo;
                public ServerInfo.ServerGroup Group;
                public GroupData(ServerInfo.ServerGroup group, DateTime dateFrom)
                {
                    Group = group;
                    DateFrom = dateFrom;
                    DateTo = DateTime.MaxValue;
                }
            }

            List<GroupData> TimeTable = new List<GroupData>();

            public ServerGroupTimeTable(ServerInfo.ServerGroup startGroup, DateTime startDT)
            {
                TimeTable.Add(new GroupData(startGroup, startDT));
            }

            public void Register(ServerInfo.ServerGroup group, DateTime dateFrom)
            {
                var lastGroupData = TimeTable.Last();
                if (lastGroupData.Group != group)
                {
                    lastGroupData.DateTo = dateFrom;
                    TimeTable.Add(new GroupData(group, dateFrom));
                }
            }

            public ServerInfo.ServerGroup GetGroupForDate(DateTime dt)
            {
                GroupData[] results = TimeTable.Where(x => x.DateFrom < dt && x.DateTo > dt).ToArray();
                if (results.Length > 1) Logger.LogError("too many results found!", this);
                if (results.Length > 0)
                {
                    GroupData data = results.First();
                    return data.Group;
                }
                else
                {
                    Logger.LogInfo("did not found a server group for this result, returning unknown, DT: " + dt.ToString(), this);
                    return ServerInfo.ServerGroup.Unknown;
                }
            }
        }

        /// <summary>
        /// Get latest skill value for specific server group and player. Returns 0 if search failed or no results found.
        /// </summary>
        /// <param name="playerName">Character name, case sensitive</param>
        /// <param name="daysToLookBack">How many days back to search for</param>
        /// <param name="skillName">Name of the skill, case sensitive</param>
        /// <param name="group">Server group type</param>
        /// <returns></returns>
        public static async Task<float> GetSkillForPlayerForServerGroupAsync(string playerName, int daysToLookBack, string skillName, ServerInfo.ServerGroup group)
        {
            Dictionary<ServerInfo.ServerGroup, float> results = await GetSkillsForPlayerAsync(playerName, daysToLookBack, skillName);
            try
            {
                return results[group];
            }
            catch (Exception _e)
            {
                Logger.LogInfo("no result available for " + skillName + " search for " + playerName, THIS, _e);
                return 0;
            }
        }

        /// <summary>
        /// Get latest skill value for each server group. If no skill data for server group, result will not contain the key.
        /// On any unhandled errors will return null and log exception. "Unknown" results come from early search data where current
        /// server name is not yet known.
        /// </summary>
        /// <param name="playerName">Character name, case sensitive</param>
        /// <param name="daysToLookBack">How many days back to search for</param>
        /// <param name="skillName">Name of the skill, case sensitive</param>
        /// <returns></returns>
        public static async Task<Dictionary<ServerInfo.ServerGroup, float>> GetSkillsForPlayerAsync(
            string playerName, int daysToLookBack, string skillName)
        {
            Task<Dictionary<ServerInfo.ServerGroup, float>> task = new Task<Dictionary<ServerInfo.ServerGroup, float>>((data) =>
                {
                    try
                    {
                        object[] dt = (object[])data;
                        return GetSkillsForPlayer_LambdaBugWorkaround((string)dt[0], (int)dt[1], (string)dt[2]).Result;
                    }
                    catch (Exception _e)
                    {
                        Logger.LogCritical("Problem with GetSkillsForPlayerAsync", THIS, _e);
                        return null;
                    }
                }, new object[] { playerName, daysToLookBack, skillName });
            task.Start();
            return await task;
        }

        //could not use async lambda in Task<T> for some reason
        private static async Task<Dictionary<ServerInfo.ServerGroup, float>> GetSkillsForPlayer_LambdaBugWorkaround(
            string playerName, int daysToLookBack, string skillName)
        {
            try
            {
                // build time table to identify, to which server group each skill gain belongs
                // TODO move this to method

                ServerGroupTimeTable timetable = await CreateSGTimeTable(playerName, daysToLookBack);

                // get most recent skill for each server group

                LogSearchData logsearchdata2 = new LogSearchData();
                logsearchdata2.SearchCriteria = new LogSearchData.SearchData(
                    playerName,
                    GameLogTypes.Skills,
                    DateTime.Now - TimeSpan.FromDays(daysToLookBack),
                    DateTime.Now,
                    "",
                    SearchTypes.RegexEscapedCaseIns);
                logsearchdata2 = await SearchWurmLogsAsync(logsearchdata2);

                Dictionary<ServerInfo.ServerGroup, float> results = new Dictionary<ServerInfo.ServerGroup, float>();

                string SKILL_INCREASED = skillName + " increased";
                string SKILL_DECREASED = skillName + " decreased";

                Guid searchId = Guid.NewGuid();

                // disabled, due to crashing WA on occasion, cause unknown
                //Logger.LogInfo(string.Format("Starting parsing results for {2} skill search: {0} Logging Handle: {1}",
                //    FormatSearchParams(logsearchdata2), searchId, skillName));

                foreach (var line in logsearchdata2.AllLines)
                {
                    if (line.Contains(SKILL_INCREASED) || line.Contains(SKILL_DECREASED))
                    {
                        float skill = WurmHelper.ExtractSkillLEVELFromLine(line);
                        ServerInfo.ServerGroup group = timetable.GetGroupForDate(LogSearchManager.BuildDateForMatch(line));

                        // disabled, due to crashing WA on occasion, cause unknown
                        //Logger.LogInfo(string.Format("Logging handle: [{0}]; Matched line: [{1}] to server group: [{2}]", searchId, line, group));
                        
                        if (skill > 0)
                        {
                            results[group] = skill;
                        }
                        else
                        {
                            Logger.LogError("extracted " + skillName + " skill was 0", THIS);
                        }
                    }
                }

                return results;
            }
            catch (Exception _e)
            {
                Logger.LogCritical("problem with GetSkillsForPlayer::implementation", THIS, _e);
                return null;
            }
        }

        static string FormatSearchParams(LogSearchData logsearchdata2)
        {
            if (logsearchdata2.SearchCriteria == null)
            {
                return "No search criteria";
            }
            return string.Format("Player: {2}, Log: {0}, Key: {1}, From: {3}, To: {4}",
                logsearchdata2.SearchCriteria.GameLogType, logsearchdata2.SearchCriteria.SearchKey,
                logsearchdata2.SearchCriteria.Player, logsearchdata2.SearchCriteria.TimeFrom,
                logsearchdata2.SearchCriteria.TimeTo);
        }

        /// <summary>
        /// Create a lookup table, that knows which server group player was on at given time point.
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="daysToLookBack">from DateTime.Now</param>
        /// <returns></returns>
        private static async Task<ServerGroupTimeTable> CreateSGTimeTable(string playerName, int daysToLookBack)
        {
            LogSearchData logsearchdata = new LogSearchData();
            logsearchdata.SearchCriteria = new LogSearchData.SearchData(
                playerName,
                GameLogTypes.Event,
                DateTime.Now - TimeSpan.FromDays(daysToLookBack),
                DateTime.Now,
                "",
                SearchTypes.RegexEscapedCaseIns);
            logsearchdata = await SearchWurmLogsAsync(logsearchdata);

            DateTime startDT = DateTime.Now - TimeSpan.FromDays(daysToLookBack);
            startDT = new DateTime(startDT.Year, startDT.Month, startDT.Day, 0, 0, 0);

            ServerGroupTimeTable timetable = new ServerGroupTimeTable(ServerInfo.ServerGroup.Unknown, startDT);

            foreach (var line in logsearchdata.AllLines)
            {
                if (line.Contains("You are on"))
                {
                    string server;
                    ServerInfo.ServerGroup? group = TryGetServerGroupFromLine(line, out server);
                    if (group != null)
                        timetable.Register(group.Value, LogSearchManager.BuildDateForMatch(line));
                }
            }
            return timetable;
        }

        /// <summary>
        /// Attempts to extract correct server group from a wurm log entry. If no group could be extracted,
        /// will return Unknown and out serverName will be null.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="serverName"></param>
        /// <returns></returns>
        public static ServerInfo.ServerGroup? TryGetServerGroupFromLine(string line, out string serverName)
        {
            //[15:14:17] 75 other players are online. You are on Exodus (774 totally in Wurm).
            Match match = Regex.Match(line, @"\d+ other players are online.*\. You are on (.+) \(", RegexOptions.Compiled);
            if (match.Success)
            {
                WurmState.WurmServer.ServerInfo.ServerGroup group =
                    WurmState.WurmServer.ServerInfo.GetServerGroup(match.Groups[1].Value);
                serverName = match.Groups[1].Value;
                return group;
            }
            else
            {
                serverName = null;
                Logger.LogError("could not match server name from line: " + (line ?? "NULL"), THIS);
                return null;
            }
        }

        /// <summary>
        /// Outs datetime from search result entry. Intended only for this API search results.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static bool TryParseDateTimeFromSearchResultLine(string line, out DateTime datetime)
        {
            try
            {
                datetime = new DateTime(
                    Convert.ToInt32(line.Substring(1, 4)),
                    Convert.ToInt32(line.Substring(6, 2)),
                    Convert.ToInt32(line.Substring(9, 2)),
                    Convert.ToInt32(line.Substring(14, 2)),
                    Convert.ToInt32(line.Substring(17, 2)),
                    Convert.ToInt32(line.Substring(20, 2))
                    );
                return true;
            }
            catch
            {
                datetime = new DateTime(0);
                return false;
            }
        }

        /// <summary>
        /// Will schedule force recache unless already scheduled or running
        /// </summary>
        public static void TryScheduleForceRecache()
        {
            SearcherUI.TryForceRecache();
        }
    }
}
