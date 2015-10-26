using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmLogsManager.Searcher
{
    /// <summary>
    /// Available log search types
    /// </summary>
    public enum SearchTypes { RegexEscapedCaseIns, RegexCustom }

    /// <summary>
    /// Extended utilities for SearchTypes enum
    /// </summary>
    public class SearchTypesEX
    {
        static Dictionary<string, SearchTypes> NameToEnumMap = new Dictionary<string, SearchTypes>();
        static Dictionary<SearchTypes, string> EnumToNameMap = new Dictionary<SearchTypes, string>();

        static SearchTypesEX()
        {
            {
                NameToEnumMap.Add("Match (default, case insensitive)", SearchTypes.RegexEscapedCaseIns);
                EnumToNameMap.Add(SearchTypes.RegexEscapedCaseIns, "Match (default, case insensitive)");

                NameToEnumMap.Add("Custom regular expression", SearchTypes.RegexCustom);
                EnumToNameMap.Add(SearchTypes.RegexCustom, "Custom regular expression");
            }
        }

        public static bool doesTypeExist(string par)
        {
            return NameToEnumMap.ContainsKey(par);
        }

        public static string GetNameForSearchType(SearchTypes type)
        {
            return EnumToNameMap[type];
        }

        public static SearchTypes GetSearchTypeForName(string name)
        {
            return NameToEnumMap[name];
        }

        public static string[] GetAllNames()
        {
            return NameToEnumMap.Keys.ToArray();
        }

        public static SearchTypes[] GetAllSearchTypes()
        {
            return EnumToNameMap.Keys.ToArray();
        }
    }

    /// <summary>
    /// This class is responsible for scheduling various searcher operations.
    /// </summary>
    /// <remarks>
    /// Please read README.txt before making modifications to this class
    /// </remarks>
    internal class LogSearchManager
    {
        const string THIS = "LogSearchManager";

        internal SQLiteDB SearchDB;
        Dictionary<string, LogFileSearcherV2> SearchersDict = new Dictionary<string, LogFileSearcherV2>();
        //bool Aborting = false;
        internal volatile bool incorrectLogsDir = false;

        //internal Thread ThreadWorker;
        //static LogSearchData ResultBuffer;

        //Queue<string> LoggerMessageQueue = new Queue<string>();

        ConcurrentQueue<Task<LogSearchData>> TaskQueue = new ConcurrentQueue<Task<LogSearchData>>();
        Task<LogSearchData> CurrentTask = null;

        internal void UpdateTaskQueue()
        {
            if (CurrentTask != null)
            {
                if (CurrentTask.IsCompleted)
                {
                    Logger.LogDebug("Cleaned old Task", THIS);
                    CurrentTask = null;
                }
                else if (CurrentTask.Status == TaskStatus.Created)
                {
                    CurrentTask.Start();
                    Logger.LogDebug("Started new Task", THIS);
                }
            }
            if (CurrentTask == null && TaskQueue.Count > 0)
            {
                TaskQueue.TryDequeue(out CurrentTask);
            }
        }

        internal void UpdateLoop()
        {
            UpdateTaskQueue();
        }

        internal LogSearchManager()
        {
        }

        internal void CreateCacheDB(string dirPath, bool clearExisting = false)
        {
            string filePath;

            filePath = dirPath == null ? GeneralHelper.PathCombineWithCodeBasePath("LogSearcher.s3db") : Path.Combine(dirPath, "LogSearcher.s3db");

            SearchDB = new SQLiteDB(filePath);
            if (clearExisting)
            {
                Logger.LogInfo("Clearing existing searcher DB", THIS);
                SearchDB.ClearDB();
                Logger.LogInfo("Clearing completed", THIS);
            }
        }

        internal void AddSearcher(string logfilepath, string player)
        {
            SearchersDict.Add(player, new LogFileSearcherV2(logfilepath, SearchDB));
        }

        internal void Clean()
        {
            //this.Aborting = true;
            if (SearchersDict != null)
            {
                foreach (var keyvalue in SearchersDict)
                {
                    keyvalue.Value.Abort();
                }
            }
            //if (ThreadWorker != null) ThreadWorker.Abort();
        }

        internal void Initialize()
        {
            Task<LogSearchData> initTask = new Task<LogSearchData>(() =>
            {
                Logger.LogDebug("Initializing", THIS);
                try
                {
                    //try to get log dir from system registry
                    string path = Path.Combine(WurmState.WurmClient.WurmPaths.WurmDir, @"players\");

                    if (path != null)
                    {
                        string[] dirs;
                        try
                        {
                            dirs = Directory.GetDirectories(path);
                        }
                        catch
                        {
                            dirs = new string[0];
                        }
                        Logger.__WriteLine("LogSearcher: Preparing cache, this may take a while...");
                        foreach (string dir in dirs)
                        {
                            string dirpath = dir;
                            //Logger.WriteLine("Found dir: " + dirpath);
                            string playername = dirpath.Substring(dirpath.LastIndexOf(@"\") + 1, dirpath.Length - dirpath.LastIndexOf(@"\") - 1);
                            dirpath += @"\test_logs";
                            //Logger.WriteLine("About to save dir: " + dirpath + " (player name: ["+playername+"])");
                            SearchersDict.Add(playername, new LogFileSearcherV2(dirpath, SearchDB));
                        }
                        foreach (var dict in SearchersDict)
                        {
                            this.incorrectLogsDir = dict.Value.incorrectLogsDir;
                        }
                        if (!this.incorrectLogsDir)
                        {
                            this.incorrectLogsDir = SearchersDict.Count == 0;
                        }
                        if (this.incorrectLogsDir)
                        {
                            Logger.__WriteLine("!! LogSearcher: No logs cached for at least one dir.");
                        }
                        Logger.__WriteLine("LogSearcher: Caching finished");
                        if (isForceRecaching)
                        {
                            if (ForceRecachingOwner != null)
                            {
                                ForceRecachingOwner.BeginInvoke(new FormLogSearcher.OnRecacheCompleteCallback(ForceRecachingOwner.InvokeOnRecacheComplete));
                                isForceRecaching = false;
                                ForceRecachingOwner = null;
                            }
                        }
                        Logger.LogDebug("Init completed", THIS);
                        return null;
                    }
                    else
                    {
                        Logger.LogDebug("Init completed", THIS);
                        //if all fails, throw an error into logger
                        Logger.__WriteLine("!! LogSearcher error: could not establish a valid path to Wurm logs directories");
                        return null;
                    }

                }
                catch (Exception _e)
                {
                    Logger.LogCritical("!!! LogSearcher: Unexpected exception at TrdStart_Initialize", THIS, _e);
                    return null;
                }
            });
            EnqueueNewSearchTask(initTask);
        }

        volatile bool updateScheduled = false; //do not enque updates if one is already in queue

        internal bool UpdateCache()
        {
            if (!updateScheduled)
            {
                Task<LogSearchData> updTask = new Task<LogSearchData>(() =>
                {
                    Logger.LogDebug("Update started", THIS);
                    try
                    {
                        foreach (var keyvalue in SearchersDict)
                        {
                            keyvalue.Value.UpdateCache();
                        }
                        updateScheduled = false;
                        Logger.LogDebug("Update completed", THIS);
                        return null;
                    }
                    catch (Exception _e)
                    {
                        Logger.LogDebug("Update completed", THIS);
                        Logger.LogCritical("!!! LogSearcher: Unexpected exception at TrdStart_Update", THIS, _e);
                        updateScheduled = false;
                        return null;
                    }

                });
                updateScheduled = true;
                EnqueueNewSearchTask(updTask);
                return true;
            }
            else return false;
        }

        internal Task<LogSearchData> PerformSearchAsync(LogSearchData logSearchData)
        {
            if (logSearchData.SearchCriteria == null)
            {
                throw new ArgumentNullException("SearchCriteria", "Search task cannot run without search criteria");
            }

            Task<LogSearchData> searchTask = new Task<LogSearchData>(x =>
                {
                    Logger.LogDebug("Search started", THIS);
                    try
                    {
                        // unbox the container
                        LogSearchData work_logSearchData = (LogSearchData)x;
                        // get the correct log searcher based on player
                        LogFileSearcherV2 logsearcher;
                        if (SearchersDict.TryGetValue(work_logSearchData.SearchCriteria.Player, out logsearcher))
                        {
                            //get the searcher to provide a string list of all required entries
                            //send the container to get results, then retrieve the container
                            work_logSearchData = logsearcher.GetFilteredSearchList(
                                work_logSearchData.SearchCriteria.GameLogType,
                                work_logSearchData.SearchCriteria.TimeFrom,
                                work_logSearchData.SearchCriteria.TimeTo,
                                work_logSearchData);
                        }
                        //callback and return search results in a thread safe way
                        if (work_logSearchData.CallerControl != null)
                        {
                            if (work_logSearchData.CallerControl.GetType() == typeof(FormLogSearcher))
                            {
                                FormLogSearcher ui = (FormLogSearcher)work_logSearchData.CallerControl;
                                work_logSearchData.AllLinesArray = work_logSearchData.AllLines.ToArray();
                                try
                                {
                                    Logger.LogDebug("Dispatching search results to Searcher UI", THIS);
                                    ui.BeginInvoke(new FormLogSearcher.DisplaySearchResultsCallback(ui.DisplaySearchResults), new object[] { logSearchData });
                                }
                                catch (Exception _e)
                                {
                                    Logger.LogError("!!! LogSearcher: error while trying to invoke FormLogSearcher, TrdStart_PerformSearch", THIS, _e);
                                }
                                Logger.LogDebug("Search completed", THIS);
                                return work_logSearchData;
                            }
                            else
                            {
                                Logger.LogDebug("Search completed", THIS);
                                return work_logSearchData;
                            }
                        }
                        else
                        {
                            Logger.LogDebug("Search completed", THIS);
                            return work_logSearchData;
                        }
                    }
                    catch (Exception _e)
                    {
                        Logger.LogDebug("Search completed", THIS);
                        Logger.LogCritical("!!! LogSearcher: Unexpected exception at TrdStart_PerformSearch", THIS, _e);
                        LogSearchData work_logSearchData = (LogSearchData)x;
                        if (work_logSearchData.CallerControl != null)
                        {
                            if (work_logSearchData.CallerControl.GetType() == typeof(FormLogSearcher))
                            {
                                try
                                {
                                    FormLogSearcher ui = (FormLogSearcher)work_logSearchData.CallerControl;
                                    work_logSearchData.AllLinesArray = work_logSearchData.AllLines.ToArray();
                                    Logger.LogDebug("Dispatching empty search results to Searcher UI", THIS);
                                    ui.BeginInvoke(new FormLogSearcher.DisplaySearchResultsCallback(ui.DisplaySearchResults), new object[] { logSearchData });
                                }
                                catch (Exception _ee)
                                {
                                    Logger.LogError("!!! LogSearcher: error while trying to invoke FormLogSearcher, TrdStart_PerformSearch", THIS, _ee);
                                }
                            }
                        }
                        return null;
                    }
                }, (object)logSearchData);
            EnqueueNewSearchTask(searchTask);
            return searchTask;
        }

        internal static DateTime BuildDateForMatch(string line)
        {
            DateTime matchDate;
            try
            {
                matchDate = new DateTime(
                    Convert.ToInt32(line.Substring(1, 4)),
                    Convert.ToInt32(line.Substring(6, 2)),
                    Convert.ToInt32(line.Substring(9, 2)),
                    Convert.ToInt32(line.Substring(14, 2)),
                    Convert.ToInt32(line.Substring(17, 2)),
                    Convert.ToInt32(line.Substring(20, 2))
                    );
            }
            catch
            {
                matchDate = new DateTime(0);
            }

            return matchDate;
        }

        bool isForceRecaching = false;
        FormLogSearcher ForceRecachingOwner = null;

        /// <summary>
        /// This will rebuild entire log cache
        /// </summary>
        /// <param name="control">calling Control or any of it's inheritors</param>
        /// <param name="internalCall">will not do any BeginInvoke callbacks</param>
        /// <returns></returns>
        internal bool ForceRecache(FormLogSearcher control, bool internalCall)
        {
            var forceRecacheTask = new Task<LogSearchData>(x =>
            {
                var ctrl = (FormLogSearcher)x;
                if (!internalCall) isForceRecaching = true;
                if (!internalCall) ForceRecachingOwner = ctrl;
                SearchDB.ClearDB();
                SearchersDict.Clear();
                Initialize();
                return null;
            }, (object)control);
            EnqueueNewSearchTask(forceRecacheTask);
            return true;
        }

        private void EnqueueNewSearchTask(Task<LogSearchData> task, bool blockFurtherEnqueues = false)
        {
            TaskQueue.Enqueue(task);
        }
    }
}
