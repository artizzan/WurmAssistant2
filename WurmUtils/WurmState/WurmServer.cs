using System;
using System.Ex;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using Aldurcraft.Utility;
using Aldurcraft.WurmOnline.WurmLogsManager;
using Aldurcraft.WurmOnline.WurmLogsManager.Searcher;

namespace Aldurcraft.WurmOnline.WurmState
{
    /// <summary>
    /// Provides means of obtaining some information about current state of wurm servers, such as wurm date or server uptime.
    /// </summary>
    public static class WurmServer
    {
        const string THIS = "WurmServer";

        internal struct ServerDesc
        {
            public readonly string Name;
            public readonly string Link;

            public ServerDesc(string name, string link)
            {
                Name = name;
                Link = link;
            }
        }

        static readonly ServerDesc[] ServerLinks =
        {
            new ServerDesc("Golden Valley", "http://jenn001.game.wurmonline.com/battles/stats.html"),
            new ServerDesc("Independence", "http://freedom001.game.wurmonline.com/battles/stats.html"),
            new ServerDesc("Deliverance", "http://freedom002.game.wurmonline.com/battles/stats.html"),
            new ServerDesc("Exodus", "http://freedom003.game.wurmonline.com/battles/stats.html"),
            new ServerDesc("Celebration", "http://freedom004.game.wurmonline.com/battles/stats.html"),
            new ServerDesc("Chaos", "http://wild001.game.wurmonline.com/battles/stats.html"),
            new ServerDesc("Elevation", "http://elevation.wurmonline.com/battles/stats.html"),
            new ServerDesc("Serenity", "http://serenity.wurmonline.com/battles/stats.html"),
            new ServerDesc("Desertion", "http://desertion.wurmonline.com/battles/stats.html"),
            new ServerDesc("Affliction", "http://affliction.wurmonline.com/battles/stats.html"),
            new ServerDesc("Pristine", "http://freedom005.game.wurmonline.com/battles/stats.html"),
            new ServerDesc("Release", "http://freedom006.game.wurmonline.com/battles/stats.html"),
            new ServerDesc("Xanadu", "http://freedom007.game.wurmonline.com/battles/stats.html"), 

            //no longer exists 11-02-2015, link reused for another challenge server
            new ServerDesc("Storm", "http://c001.game.wurmonline.com/battles/stats.html"), 

            new ServerDesc("Cauldron", "http://c001.game.wurmonline.com/battles/stats.html"), 
        };

        public enum UpdateSource { Unspecified, WebFeed, WurmLogs }

        [DataContract]
        internal struct WurmDateTimeInfoPair
        {
            [DataMember]
            public readonly DateTime Stamp;
            [DataMember]
            public readonly WurmDateTime WDT_Value;
            [DataMember]
            public readonly DateTime AdjustedStamp;

            public WurmDateTimeInfoPair(DateTime stamp, WurmDateTime wdt_value)
            {
                Stamp = stamp;
                try
                {
                    AdjustedStamp = Stamp - TimeSpan.FromDays(2);
                }
                catch (ArgumentOutOfRangeException)
                {
                    AdjustedStamp = DateTime.MinValue;
                }
                WDT_Value = wdt_value;
            }

            public WurmDateTime AdjValue
            {
                get
                {
                    //adjust returned value for the time that passed, this is always slightly off
                    TimeSpan ts = (DateTime.Now - Stamp);
                    TimeSpan tsadj = ts.Multiply(WurmDateTime.WurmTimeToRealTimeRatio);
                    WurmDateTime wdt = WDT_Value + tsadj;
                    return wdt;
                }
            }
        }

        [DataContract]
        internal struct WurmUptimeInfoPair
        {
            [DataMember]
            public readonly DateTime Stamp;
            [DataMember]
            public readonly TimeSpan Uptime_Value;
            [DataMember]
            public readonly DateTime AdjustedStamp;

            public WurmUptimeInfoPair(DateTime stamp, TimeSpan uptime_value)
            {
                Stamp = stamp;
                try
                {
                    AdjustedStamp = Stamp - TimeSpan.FromDays(2);
                }
                catch (ArgumentOutOfRangeException)
                {
                    AdjustedStamp = DateTime.MinValue;
                }
                Uptime_Value = uptime_value;
            }

            public TimeSpan AdjValue
            {
                get
                {
                    return Uptime_Value + (DateTime.Now - Stamp);
                }
            }
        }

        [DataContract]
        internal class WurmServerInfo
        {
            //lock when init and serialize, lock free read
            [DataMember]
            object lock_obj = new object();

            [DataMember]
            WurmDateTimeInfoPair WDT_web = new WurmDateTimeInfoPair(DateTime.MinValue, WurmDateTime.MinValue);
            [DataMember]
            WurmDateTimeInfoPair WDT_logs = new WurmDateTimeInfoPair(DateTime.MinValue, WurmDateTime.MinValue);

            [DataMember]
            WurmUptimeInfoPair Uptime_web = new WurmUptimeInfoPair(DateTime.MinValue, TimeSpan.Zero);
            [DataMember]
            WurmUptimeInfoPair Uptime_logs = new WurmUptimeInfoPair(DateTime.MinValue, TimeSpan.Zero);

            /// <summary>
            /// update uptime if new data is more up to date
            /// </summary>
            /// <param name="stamp"></param>
            /// <param name="value"></param>
            /// <param name="source"></param>
            internal void SetUptime(DateTime stamp, TimeSpan value, UpdateSource source)
            {
                Logger.LogDebug(String.Format("{0} ; {1} ; {2} ; {3}", stamp, value, source, lock_obj));
                lock (lock_obj)
                {
                    if (source == UpdateSource.WebFeed)
                    {
                        if (stamp > Uptime_web.Stamp)
                        {
                            Uptime_web = new WurmUptimeInfoPair(stamp, value);
                        }
                    }
                    else if (source == UpdateSource.WurmLogs)
                    {
                        if (stamp > Uptime_logs.Stamp)
                        {
                            Uptime_logs = new WurmUptimeInfoPair(stamp, value);
                        }
                    }
                }
            }

            /// <summary>
            /// update wurm date time if new data is more up to date
            /// </summary>
            /// <param name="stamp"></param>
            /// <param name="value"></param>
            /// <param name="source"></param>
            internal void SetWurmDateTime(DateTime stamp, WurmDateTime value, UpdateSource source)
            {
                lock (lock_obj)
                {
                    if (source == UpdateSource.WebFeed)
                    {
                        if (stamp > WDT_web.Stamp)
                        {
                            WDT_web = new WurmDateTimeInfoPair(stamp, value);
                        }
                    }
                    else if (source == UpdateSource.WurmLogs)
                    {
                        if (stamp > WDT_logs.Stamp)
                        {
                            WDT_logs = new WurmDateTimeInfoPair(stamp, value);
                        }
                    }
                }
            }

            internal TimeSpan GetLatestUptime()
            {
                // lets try no lock for now, ref exchanges are atomic, result value will always be valid
                //lock (lock_obj)
                {
                    // artificially adjusting web timestamp by 2 days to make WA rely on them 
                    // only in case latest log data is not available for any reason
                    if (Uptime_web.AdjustedStamp > Uptime_logs.Stamp)
                        return Uptime_web.AdjValue;
                    else
                        return Uptime_logs.AdjValue;
                }
            }

            internal WurmDateTime GetLatestWDT()
            {
                //lock (lock_obj)
                {
                    // same as above
                    if (WDT_web.AdjustedStamp > WDT_logs.Stamp)
                        return WDT_web.AdjValue;
                    else
                        return WDT_logs.AdjValue;
                }
            }

            internal DateTime GetLastServerReset()
            {
                return DateTime.Now - GetLatestUptime();
            }
        }

        [DataContract]
        internal class WurmServerData
        {
            [DataMember]
            internal ConcurrentDictionary<string, WurmServerInfo> Data = new ConcurrentDictionary<string, WurmServerInfo>();
        }

        public class NoDataException : Exception
        {
            public NoDataException(string message) : base(message) { }
        }

        /// <summary>
        /// If WurmServer should try to contact official Wurm server feeds for latest info about server.
        /// </summary>
        public static bool WebFeedDisabled { get; set; }

        static WurmServerData _serverData;
        static Task _initTask;

        static string _dataFilePath = null;

        static bool isInitialized = false;

        /// <summary>
        /// Init WurmServer with optional dir path to where it should store it's data
        /// </summary>
        /// <param name="dataDir">Absolute directory path, default null uses CodeBase</param>
        /// <exception cref="ArgumentException">Directory path was probably invalid</exception>
        /// <exception cref="InvalidOperationException">WurmServer was already initialized</exception>
        public static void InitializeWurmServer(string dataDir = null)
        {
            if (isInitialized) throw new InvalidOperationException("WurmServer is already initialized");

            if (dataDir == null) _dataFilePath = GeneralHelper.PathCombineWithCodeBasePath("WurmServerData.xml");
            else _dataFilePath = Path.Combine(dataDir, "WurmServerData.xml");

            Logger.LogInfo("Initializing", THIS);
            _initTask = new Task(async () =>
                {
                    try
                    {
                        //get the old data so program can init faster
                        var ds = new NetDataContractSerializer();
                        using (Stream s = File.OpenRead(_dataFilePath))
                        {
                            _serverData = (WurmServerData)ds.ReadObject(s);
                        }
                    }
                    catch (Exception _e)
                    {
                        _serverData = new WurmServerData();
                        Logger.LogInfo("problem while loading cached WurmServer state, recaching", THIS, _e);
                    }
                    //var webTask = ExtractFromWebAsync();
                    var logsTask = ExtractFromLogsAsync();
                    //await webTask;
                    await logsTask;
                    Logger.LogInfo("Init complete", THIS);
                });
            _initTask.Start();
        }

        static volatile bool _refreshingDateTimeAndUptimeFromWeb = false;
        static async Task ExtractFromWebAsync()
        {
            if (WebFeedDisabled)
            {
                Logger.LogInfo("Skipping ExtractFromWebAsync due user setting");
                return;
            }

            _refreshingDateTimeAndUptimeFromWeb = true;
            Logger.LogDiag("ExtractFromWebAsync sync flag set");
            //update ServerData with any info from the web
            Logger.LogDebug("Debug: http worker start", THIS);

            var workerTasks = new List<Task>();

            foreach (var link in ServerLinks)
            {
                var localLink = link;

                var tsk = new Task(() =>
                {
                    try
                    {
                        var req = (HttpWebRequest)WebRequest.Create(localLink.Link);
                        req.Timeout = 15000;
                        var res = (HttpWebResponse)req.GetResponse();
                        DateTime headerLastUpdated = res.LastModified;
                        DateTime TimeParsed = DateTime.Now;

                        var httpLines = new List<string>();

                        using (Stream stream = res.GetResponseStream())
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                bool serverNameRead = false;
                                bool uptimeRead = false;
                                bool wurmTimeRead = false;
                                string line;
                                string name = string.Empty,
                                    strUptime = string.Empty,
                                    strWurmDateTime = string.Empty;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    if (serverNameRead)
                                    {
                                        Match match = Regex.Match(line, @">.+<");
                                        name = match.Value.Substring(1, match.Value.Length - 2);
                                        serverNameRead = false;
                                    }
                                    else if (uptimeRead)
                                    {
                                        Match match = Regex.Match(line, @">.+<");
                                        strUptime = match.Value.Substring(1, match.Value.Length - 2);
                                        uptimeRead = false;
                                    }
                                    else if (wurmTimeRead)
                                    {
                                        Match match = Regex.Match(line, @">.+<");
                                        strWurmDateTime = match.Value.Substring(1, match.Value.Length - 2);
                                        wurmTimeRead = false;
                                    }

                                    httpLines.Add(line);
                                    if (Regex.IsMatch(line, "Server name"))
                                    {
                                        serverNameRead = true;
                                    }
                                    else if (Regex.IsMatch(line, "Uptime"))
                                    {
                                        uptimeRead = true;
                                    }
                                    else if (Regex.IsMatch(line, "Wurm Time"))
                                    {
                                        wurmTimeRead = true;
                                    }
                                }
                                if (!string.IsNullOrEmpty(name.Trim()))
                                {
                                    name = name.Trim().ToLower();
                                    WurmServerInfo info = _serverData.Data.GetOrAdd(name, new WurmServerInfo());

                                    // Rolf has a time machine and it causes assistant to suffer temporal distortions
                                    // this is a hackfix, and I'm not going to waste more time working around their slopyness
                                    DateTime dtnow = DateTime.Now;
                                    if (headerLastUpdated > dtnow)
                                    {
                                        Logger.LogInfo(string.Format("WebFeed: Header timestamp '{0}' arrived from the future, adjusting to match DateTime.Now '{1}'",
                                            headerLastUpdated, dtnow), THIS);
                                        headerLastUpdated = dtnow;
                                    }
                                    TimeSpan uptime = GetTimeSpanFromUptimeWebString(strUptime);

                                    Logger.LogInfo(string.Format("WebFeed: Trying to update web-uptime for '{0}' to '{1}' with timestamp: '{2}'",
                                        name, uptime, headerLastUpdated), THIS);
                                    info.SetUptime(headerLastUpdated, uptime, UpdateSource.WebFeed);

                                    WurmDateTime wdt = GetWurmDateTimeFromWDTWebString(strWurmDateTime);
                                    Logger.LogInfo(string.Format("WebFeed: Trying to update web-wurmdate for '{0}' to '{1}' with timestamp: '{2}'",
                                        name, wdt, headerLastUpdated), THIS);
                                    info.SetWurmDateTime(headerLastUpdated, wdt, UpdateSource.WebFeed);
                                }
                            }
                        }
                    }
                    catch (Exception _e)
                    {
                        Logger.LogInfo(
                            string.Format(
                                "! TimingAssist: There was an exception when accessing / parsing http data for {0}, link: {1}",
                                localLink.Name ?? "Null",
                                localLink.Link),
                            THIS,
                            _e);
                    }
                });

                workerTasks.Add(tsk);
                tsk.Start();
            }

            await TaskEx.WhenAll(workerTasks.ToArray());

            // note: there been issues (one user) with running all requests simultaneously
            // update: converting to synchronous task seems ineffective, likely something outside WA scope is responsible for that issue

            SaveData();
            TriggerPotentialChangesEvent();

            _refreshingDateTimeAndUptimeFromWeb = false;
            Logger.LogDiag("ExtractFromWebAsync sync flag unset");
        }

        static void TriggerPotentialChangesEvent()
        {
            // send events that data may have updated
            var evnt = OnPotentialWurmDateAndUptimeChange;
            if (evnt != null)
                evnt(THIS, new EventArgs());
        }

        static TimeSpan GetTimeSpanFromUptimeWebString(string webString)
        {
            //EX:   The server has been up 1 days, 14 hours and 43 minutes.
            Match matchdays = Regex.Match(webString, @"\d\d* days");
            Match matchhours = Regex.Match(webString, @"\d\d* hours");
            Match matchminutes = Regex.Match(webString, @"\d\d* minutes");
            Match matchseconds = Regex.Match(webString, @"\d\d* seconds");

            int days = GeneralHelper.MatchToInt32(matchdays);
            int hours = GeneralHelper.MatchToInt32(matchhours);
            int minutes = GeneralHelper.MatchToInt32(matchminutes);
            int seconds = GeneralHelper.MatchToInt32(matchseconds);

            Logger.LogDebug("[" + days + "][" + hours + "][" + minutes + "][" + seconds + "]", THIS);

            return new TimeSpan(days, hours, minutes, 0);
        }

        static WurmDateTime GetWurmDateTimeFromWDTWebString(string logline)
        {
            //time
            Match wurmTime = Regex.Match(logline, @" \d\d:\d\d:\d\d ");
            int hour = Convert.ToInt32(wurmTime.Value.Substring(1, 2));
            int minute = Convert.ToInt32(wurmTime.Value.Substring(4, 2));
            int second = Convert.ToInt32(wurmTime.Value.Substring(7, 2));
            //day
            WurmDay? day = null;
            foreach (string name in WurmDayEX.WurmDaysNames)
            {
                if (Regex.IsMatch(logline, name))
                {
                    day = WurmDayEX.GetEnumForName(name);
                    break;
                }
            }
            //week
            Match wurmWeek = Regex.Match(logline, @"week (\d)");
            int week = Convert.ToInt32(wurmWeek.Groups[1].Value);
            //month(starfall)
            WurmStarfall? starfall = null;
            foreach (string name in WurmStarfallEX.WurmStarfallNames)
            {
                if (Regex.IsMatch(logline, name))
                {
                    starfall = WurmStarfallEX.GetEnumForName(name);
                    break;
                }
            }
            //year
            Match wurmYear = Regex.Match(logline, @"in the year of (\d+)");
            int year = Convert.ToInt32(wurmYear.Groups[1].Value);

            if (day == null || starfall == null) throw new Exception("log line was not parsed correctly into day or starfall: " + (logline ?? "NULL"));

            return new WurmDateTime(year, starfall.Value, week, day.Value, hour, minute, second);
        }

        static async Task ExtractFromLogsAsync()
        {
            await WurmLogSearcherAPI.AwaitInitializationAsync(); //once logsearcher is initialized

            //update ServerData with any info from log searching

            var names = WurmClient.WurmPaths.GetAllPlayersNames();

            foreach (var name in names)
            {
                var lgs = await WurmLogSearcherAPI.SearchWurmLogsAsync(new LogSearchData()
                {
                    SearchCriteria = new LogSearchData.SearchData(
                        name, GameLogTypes.Event, DateTime.Now - TimeSpan.FromDays(3), DateTime.Now, "",
                        SearchTypes.RegexEscapedCaseIns),
                });

                string currentServer = null;
                foreach (var line in lgs.AllLines)
                {
                    //[15:14:17] 75 other players are online. You are on Exodus (774 totally in Wurm).
                    if (line.Contains("You are on"))
                    {
                        Match match = Regex.Match(line, @"\d+ other players are online.+\. You are on (.+) \(", RegexOptions.Compiled);
                        if (match.Success)
                        {
                            currentServer = match.Groups[1].Value;
                        }
                    }

                    if (currentServer != null)
                    {
                        await TryUpdateUptime(line, currentServer, name, false);
                        await TryUpdateWurmDateTime(line, currentServer, name, false);
                    }
                }
            }

            SaveData();
            TriggerPotentialChangesEvent();
        }

        static async Task TryUpdateUptime(string line, string currentServer, string playerName, bool liveLogs = false)
        {
            //[15:14:22] The server has been up 2 days, 3 hours and 23 minutes.
            if (line.Contains("The server has been up"))
            {
                DateTime stamp;
                if (liveLogs) stamp = DateTime.Now;
                else if (!WurmLogSearcherAPI.TryParseDateTimeFromSearchResultLine(line, out stamp))
                {
                    Logger.LogError("problem parsing timestamp from search result, line: " + line, THIS);
                    return;
                }

                try
                {
                    var uptime = GetTimeSpanServerUpSince(line);

                    if (currentServer == null)
                    {
                        await PlayerServerTracker.InitializedTask;
                        currentServer = await PlayerServerTracker.GetCurrentServerForPlayerAsync(playerName);
                    }

                    if (currentServer == null)
                    {
                        Logger.LogError(
                            string.Format(
                                "TryUpdateUptime > currentServer is null, skipping update. For player: {0}. Processing line: {1}. LiveLogs: {2}",
                                playerName, line, liveLogs), THIS);
                    }
                    else
                    {
                        var srvname = currentServer.Trim().ToLower();
                        WurmServerInfo info = _serverData.Data.GetOrAdd(srvname, new WurmServerInfo());

                        Logger.LogInfo(
                            string.Format(
                                "Setting uptime from log search result for '{0}' to '{1}' with timestamp: '{2}'",
                                srvname, uptime, stamp), THIS);
                        //SetUptime will only update if stamp is earlier than existing info
                        info.SetUptime(stamp, uptime, UpdateSource.WurmLogs);

                        if (liveLogs)
                        {
                            SaveData();
                            TriggerPotentialChangesEvent();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("", THIS, ex);
                }

            }
        }

        static async Task TryUpdateWurmDateTime(string line, string currentServer, string playerName, bool liveLogs = false)
        {
            //[16:24:19] It is 09:00:48 on day of the Wurm in week 4 of the Bear's starfall in the year of 1035.
            if (line.Contains("It is "))
            {
                if (Regex.IsMatch(line, @"It is \d\d:\d\d:\d\d on .+ in week .+ in the year of \d+\.", RegexOptions.Compiled))
                {
                    DateTime stamp;
                    if (liveLogs) stamp = DateTime.Now;
                    else if (!WurmLogSearcherAPI.TryParseDateTimeFromSearchResultLine(line, out stamp))
                    {
                        Logger.LogError("problem parsing wurm date from search result, line: " + line, THIS);
                        return;
                    }

                    try
                    {
                        var wurmDateTime = WurmDateTime.CreateFromLogLine(line);

                        if (currentServer == null)
                        {
                            await PlayerServerTracker.InitializedTask;
                            currentServer = await PlayerServerTracker.GetCurrentServerForPlayerAsync(playerName);
                        }
                        if (currentServer == null)
                        {
                            Logger.LogError(
                                string.Format(
                                    "TryUpdateWurmDateTime > currentServer is null, skipping update. For player: {0}. Processing line: {1}. LiveLogs: {2}",
                                    playerName, line, liveLogs), THIS);
                        }
                        else
                        {
                            var srvname = currentServer.Trim().ToLower();
                            WurmServerInfo info = _serverData.Data.GetOrAdd(srvname, new WurmServerInfo());

                            Logger.LogInfo(
                                string.Format(
                                    "Setting wurm date from log search result for '{0}' to '{1}' with timestamp: '{2}'",
                                    srvname, wurmDateTime, stamp), THIS);
                            //SetWurmDateTime will only update if stamp is earlier than existing info
                            info.SetWurmDateTime(stamp, wurmDateTime, UpdateSource.WurmLogs);

                            if (liveLogs)
                            {
                                SaveData();
                                TriggerPotentialChangesEvent();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("", THIS, ex);
                    }
                }
            }
        }

        static TimeSpan GetTimeSpanServerUpSince(string logevent)
        {
            //EX:   The server has been up 1 days, 14 hours and 43 minutes.
            Match matchdays = Regex.Match(logevent, @"(\d\d*) days", RegexOptions.Compiled);
            int days = ParseMatchToInt32(matchdays);
            Match matchhours = Regex.Match(logevent, @"(\d\d*) hours", RegexOptions.Compiled);
            int hours = ParseMatchToInt32(matchhours);
            Match matchminutes = Regex.Match(logevent, @"(\d\d*) minutes", RegexOptions.Compiled);
            int minutes = ParseMatchToInt32(matchminutes);
            Match matchseconds = Regex.Match(logevent, @"(\d\d*) seconds", RegexOptions.Compiled);
            int seconds = ParseMatchToInt32(matchseconds);

            Debug.WriteLine("[" + days + "][" + hours + "][" + minutes + "][" + seconds + "]");

            if (days + hours + minutes + seconds == 0) throw new Exception("Server uptime equals 0 seconds");

            return new TimeSpan(days, hours, minutes, seconds);
        }

        internal static void HandleNewLogEvent(string line, string playerName)
        {
            TryUpdateUptime(line, null, playerName, true);
            TryUpdateWurmDateTime(line, null, playerName, true);
        }

        static int ParseMatchToInt32(Match match)
        {
            if (match.Success)
            {
                return Int32.Parse(match.Groups[1].Value);
            }
            else return 0;
        }

        private static readonly object _saveDataLocker = new object();
        static void SaveData()
        {
            lock (_saveDataLocker)
            {
                try
                {
                    var ds = new NetDataContractSerializer();
                    using (Stream s = File.Create(_dataFilePath))
                    {
                        ds.WriteObject(s, _serverData);
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(_dataFilePath));
                        SaveData();
                    }
                    catch (Exception _e)
                    {
                        Logger.LogError("invalid save path?", THIS, _e);
                    }
                }
                catch (Exception _e)
                {
                    Logger.LogError("there was a problem with save server data", THIS, _e);
                }
            }
        }

        private static TimeSpan GetUptime(string serverName)
        {
            return GetServerInfo(serverName).GetLatestUptime();
        }

        private static DateTime GetLastServerReset(string serverName)
        {
            return GetServerInfo(serverName).GetLastServerReset();
        }

        private static WurmDateTime GetWurmDateTime(string serverName)
        {
            return GetServerInfo(serverName).GetLatestWDT();
        }

        private static WurmServerInfo GetServerInfo(string serverName)
        {
            serverName = serverName.Trim().ToLower();
            WurmServerInfo wsd;
            if (_serverData.Data.TryGetValue(serverName, out wsd))
            {
                return wsd;
            }
            else throw new NoDataException("No data available for server: " + (serverName ?? "NULL"));
        }

        #region PUBLIC API

        /// <summary>
        /// Outputs server uptime for specified server. Returns false and TimeSpan.Zero if no uptime available.
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static bool TryGetUptime(string serverName, out TimeSpan ts)
        {
            serverName = serverName.Trim().ToLower();
            if (!_initTask.IsCompleted)
            {
                ts = TimeSpan.Zero;
                return false;
            }
            else
            {
                WurmServerInfo wsd;
                if (_serverData.Data.TryGetValue(serverName, out wsd))
                {
                    ts = wsd.GetLatestUptime();
                    return true;
                }
                else
                {
                    ts = TimeSpan.Zero;
                    return false;
                }
            }
        }

        /// <summary>
        /// Outputs wurm date for specified server. Returns false and TimeSpan.Zero if no date available.
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="wdt"></param>
        /// <returns></returns>
        public static bool TryGetWurmDateTime(string serverName, out WurmDateTime wdt)
        {
            if (!_initTask.IsCompleted)
            {
                wdt = WurmDateTime.MinValue;
                return false;
            }
            else
            {
                WurmServerInfo wsd;
                if (_serverData.Data.TryGetValue(serverName, out wsd))
                {
                    wdt = wsd.GetLatestWDT();
                    return true;
                }
                else
                {
                    wdt = WurmDateTime.MinValue;
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns uptime for specified server, will await until data is aquired from web feeds.
        /// </summary>
        /// <param name="serverName">server name, case insensitive</param>
        /// <exception cref="WurmState.WurmServer.NoDataException">no data was available for this server</exception>
        /// <returns></returns>
        public static async Task<TimeSpan> GetUptimeAsync(string serverName)
        {
            if (!_initTask.IsCompleted) await _initTask;
            return GetUptime(serverName);
        }

        /// <summary>
        /// Returns date and time when this server was last launched.
        /// </summary>
        /// <param name="serverName">server name, case insensitive</param>
        /// <exception cref="WurmState.WurmServer.NoDataException">no data was available for this server</exception>
        /// <returns></returns>
        public static async Task<DateTime> GetLastServerResetAsync(string serverName)
        {
            if (!_initTask.IsCompleted) await _initTask;
            return GetLastServerReset(serverName);
        }

        /// <summary>
        /// returns wurm date for specified server, will await until data is aquired from web feeds.
        /// </summary>
        /// <param name="serverName">server name, case insensitive</param>
        /// <exception cref="WurmState.WurmServer.NoDataException">no data was available for this server</exception>
        /// <returns></returns>
        public static async Task<WurmDateTime> GetWurmDateTimeAsync(string serverName)
        {
            if (!_initTask.IsCompleted) await _initTask;
            return GetWurmDateTime(serverName);
        }

        /// <summary>
        /// Returns all server names for which data is available, will await until data is aquired from web feeds.
        /// </summary>
        /// <returns></returns>
        public static async Task<string[]> GetAllServerNamesAsync()
        {
            if (!_initTask.IsCompleted) await _initTask;
            return _serverData.Data.Keys.ToArray();
        }

        /// <summary>
        /// Attempts to refresh wurm servers date and uptime info if not already refreshing,
        /// once finished, triggers OnPotentialWurmDateAndUptimeChange event
        /// </summary>
        /// <returns></returns>
        public static async Task RefreshDateTimeAndUptime()
        {
            try
            {
                if (!_refreshingDateTimeAndUptimeFromWeb)
                {
                    await ExtractFromWebAsync();
                }
            }
            catch (Exception _e)
            {
                Logger.LogError("problem with RefreshDateTimeAndUptime", THIS, _e);
            }
        }

        /// <summary>
        /// Triggered after wurm date and uptime info was refreshed from web feed, also during Init
        /// </summary>
        public static event EventHandler OnPotentialWurmDateAndUptimeChange;

        #endregion

        public static class ServerInfo
        {
            public enum ServerGroup { Unknown=0, Freedom=1, Epic=2, Challenge=3 }

            static Dictionary<string, ServerGroup> ServerNameToGroup = new Dictionary<string, ServerGroup>();

            static HashSet<ServerGroup> AllServerGroups = new HashSet<ServerGroup>();

            static ServerInfo()
            {
                ServerNameToGroup.Add("GOLDEN VALLEY", ServerGroup.Unknown);

                ServerNameToGroup.Add("DELIVERANCE", ServerGroup.Freedom);
                ServerNameToGroup.Add("PRISTINE", ServerGroup.Freedom);
                ServerNameToGroup.Add("INDEPENDENCE", ServerGroup.Freedom);
                ServerNameToGroup.Add("EXODUS", ServerGroup.Freedom);
                ServerNameToGroup.Add("CELEBRATION", ServerGroup.Freedom);
                ServerNameToGroup.Add("RELEASE", ServerGroup.Freedom);
                ServerNameToGroup.Add("CHAOS", ServerGroup.Freedom);
                ServerNameToGroup.Add("XANADU", ServerGroup.Freedom);

                ServerNameToGroup.Add("ELEVATION", ServerGroup.Epic);
                ServerNameToGroup.Add("SERENITY", ServerGroup.Epic);
                ServerNameToGroup.Add("AFFLICTION", ServerGroup.Epic);
                ServerNameToGroup.Add("DESERTION", ServerGroup.Epic);

                //no longer exists 11-02-2015
                ServerNameToGroup.Add("STORM", ServerGroup.Challenge); 
                
                ServerNameToGroup.Add("CAULDRON", ServerGroup.Challenge);

                ServerGroup[] groups = (ServerGroup[])Enum.GetValues(typeof(ServerGroup));
                foreach (var group in groups)
                {
                    AllServerGroups.Add(group);
                }
            }

            /// <summary>
            /// Returns group associated with this server name or Unknown if name not recognized.
            /// </summary>
            /// <param name="serverName"></param>
            /// <returns></returns>
            public static ServerGroup GetServerGroup(string serverName)
            {
                if (serverName == null) return ServerGroup.Unknown;

                ServerGroup result;
                if (ServerNameToGroup.TryGetValue(serverName.ToUpperInvariant(), out result))
                {
                    return result;
                }
                else return ServerGroup.Unknown;
            }

            /// <summary>
            /// Returns all supported server groups.
            /// </summary>
            /// <returns></returns>
            public static ServerGroup[] GetAllServerGroups()
            {
                return AllServerGroups.ToArray();
            }


        }
    }
}
