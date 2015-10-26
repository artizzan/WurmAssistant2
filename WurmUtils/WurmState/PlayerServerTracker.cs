using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Aldurcraft.Utility;
using Aldurcraft.WurmOnline.WurmLogsManager;
using Aldurcraft.WurmOnline.WurmLogsManager.Searcher;

namespace Aldurcraft.WurmOnline.WurmState
{
    public static class PlayerServerTracker
    {
        [DataContract]
        class StateInfo
        {
            [DataMember]
            private string _playerName;
            [DataMember]
            internal DateTime LastLogSearch { get; set; }

            [DataMember] internal ServerGroupsManager ServerGroupsManager;

            [DataMember]
            private string _serverName;
            internal string ServerName
            {
                get { return _serverName; }
                set { _serverName = value; }
            }

            internal Task LogSearchingTask;

            internal bool NeedSaving { get; private set; }

            public bool ServerEstablished { get; set; }
            public bool LogSearchFinished { get; private set; }

            internal StateInfo(string playerName, string serverName, bool serverEstablished = false)
            {
                _playerName = playerName;
                _serverName = serverName;
                ServerEstablished = serverEstablished;
            }

            public StateInfo(string playerName)
            {
                this._playerName = playerName;
                Init();
            }

            [OnDeserializing]
            void OnDes(StreamingContext context)
            {
                Init();
            }

            private void Init()
            {
                _serverName = null;
                LastLogSearch = DateTime.MinValue;
                ServerGroupsManager = new ServerGroupsManager();

                LogSearchingTask = new Task(SearchLogs);
            }

            private void SearchLogs()
            {
                DateTime searchFrom = LastLogSearch;
                if (DateTime.Now - searchFrom > TimeSpan.FromDays(60))
                {
                    searchFrom = DateTime.Now - TimeSpan.FromDays(60);
                }
                else if (DateTime.Now - searchFrom < TimeSpan.FromDays(2))
                {
                    searchFrom = DateTime.Now - TimeSpan.FromDays(2);
                }

                var lgs = new LogSearchData
                {
                    SearchCriteria = new LogSearchData.SearchData(
                        _playerName,
                        GameLogTypes.Event,
                        searchFrom,
                        DateTime.Now,
                        "",
                        SearchTypes.RegexEscapedCaseIns)
                };

                var task = WurmLogSearcherAPI.SearchWurmLogsAsync(lgs);
                task.Wait();
                lgs = task.Result;

                //parse results
                string lastServerName = null;
                foreach (var line in lgs.AllLines)
                {
                    var result = PlayerServerTracker.MatchServerFromLogLine(line);
                    if (result != null)
                    {
                        lastServerName = result;

                        string useless;
                        var sg = WurmLogSearcherAPI.TryGetServerGroupFromLine(line, out useless);
                        if (sg != null)
                        {
                            DateTime thisLineStamp;
                            if (WurmLogSearcherAPI.TryParseDateTimeFromSearchResultLine(line, out thisLineStamp))
                            {
                                ServerGroupsManager.Update(sg.Value, thisLineStamp, result);
                            }
                            else Logger.LogError("Could not process timestamp from line? "+thisLineStamp);
                        }
                        else Logger.LogError("Unknown server? "+line, this);
                    }
                }

                // checking if real-time log events didn't set this field already
                if (!ServerEstablished)
                {
                    if (lastServerName != null) _serverName = lastServerName;
                    ServerEstablished = true;
                }

                LastLogSearch = DateTime.Now;

                NeedSaving = true;

                LogSearchFinished = true;
                ServerEstablished = true;
            }

            internal void UpdateSgFromLiveLogs(string serverName)
            {
                DateTime stamp = DateTime.Now;
                WurmState.WurmServer.ServerInfo.ServerGroup group =
                    WurmState.WurmServer.ServerInfo.GetServerGroup(serverName);

                ServerGroupsManager.Update(group, stamp, serverName);
            }
        }

        [DataContract]
        class PlayerServerTrackerState
        {
            private object _locker;

            internal bool NeedSaving { get; set; }

            [DataMember]
            private Dictionary<string, StateInfo> _playerToServerMap;

            internal PlayerServerTrackerState()
            {
                Init();
            }

            [OnDeserializing]
            void OnDes(StreamingContext context)
            {
                Init();
            }

            private void Init()
            {
                _locker = new object();

                lock (_locker)
                {
                    _playerToServerMap = new Dictionary<string, StateInfo>();
                }

            }

            internal string GetStatusFast(string playerName)
            {
                StateInfo stateInfo;
                if (_playerToServerMap.TryGetValue(playerName, out stateInfo))
                {
                    //multithreading access requires null check
                    //no lock is safe because we will always find some server name here
                    if (stateInfo != null)
                    {
                        return stateInfo.ServerName;
                    }
                }
                return null;
            }

            internal async Task<string> GetStatusForCurrentServerGroupAsync(string playerName)
            {
                StateInfo stateInfo;
                lock (_locker)
                {
                    if (!_playerToServerMap.TryGetValue(playerName, out stateInfo))
                    {
                        stateInfo = new StateInfo(playerName);
                        _playerToServerMap.Add(playerName, stateInfo);
                    }

                    if (stateInfo.ServerEstablished) return stateInfo.ServerName;
                    else
                    {
                        try
                        {
                            stateInfo.LogSearchingTask.Start();
                        }
                        catch (InvalidOperationException)
                        {
                            // is already started
                        }
                    }
                }

                await stateInfo.LogSearchingTask;

                if (stateInfo.NeedSaving) this.NeedSaving = true;

                return stateInfo.ServerName;
            }

            internal void SetStatus(string playerName, string serverName)
            {
                lock (_locker)
                {
                    StateInfo stateInfo;
                    if (!_playerToServerMap.TryGetValue(playerName, out stateInfo))
                    {
                        stateInfo = new StateInfo(playerName, serverName, true);
                        stateInfo.UpdateSgFromLiveLogs(serverName);
                        _playerToServerMap.Add(playerName, stateInfo);
                    }
                    else
                    {
                        stateInfo.ServerEstablished = true;
                        stateInfo.ServerName = serverName;
                        stateInfo.UpdateSgFromLiveLogs(serverName);
                    }
                    _settings.Save();
                }
            }

            internal async Task<string> GetStatusForServerGroupAsync(string playerName, WurmServer.ServerInfo.ServerGroup sg)
            {
                StateInfo stateInfo;
                lock (_locker)
                {
                    if (!_playerToServerMap.TryGetValue(playerName, out stateInfo))
                    {
                        stateInfo = new StateInfo(playerName);
                        _playerToServerMap.Add(playerName, stateInfo);
                    }

                    if (stateInfo.LogSearchFinished)
                    {
                        return stateInfo.ServerGroupsManager.GetCurrentServerForGroup(sg);
                    }
                    else
                    {
                        try
                        {
                            stateInfo.LogSearchingTask.Start();
                        }
                        catch (InvalidOperationException)
                        {
                            // is already started
                        }
                    }
                }

                await stateInfo.LogSearchingTask;

                if (stateInfo.NeedSaving) this.NeedSaving = true;

                return stateInfo.ServerGroupsManager.GetCurrentServerForGroup(sg);
            }
        }

        private static PersistentObject<PlayerServerTrackerState> _settings;

        private static TaskCompletionSource<bool> _initializedTcs = new TaskCompletionSource<bool>();

        public static Task<bool> InitializedTask
        {
            get { return _initializedTcs.Task; }
        }

        /// <summary>
        /// initialize this class by providing directory path to where cache should be saved
        /// </summary>
        /// <param name="dataDir"></param>
        public static void Initialize(string dataDir)
        {
            _settings = new PersistentObject<PlayerServerTrackerState>(new PlayerServerTrackerState());
            _settings.SetFilePathAndLoad(Path.Combine(dataDir, "PlayerServerTrackerState.xml"));
            _initializedTcs.SetResult(true);
        }

        /// <summary>
        /// returns null if no server available, player name is case insensitive
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public static async Task<string> GetCurrentServerForPlayerAsync(string playerName)
        {
            await _initializedTcs.Task;

            var result = await _settings.Value.GetStatusForCurrentServerGroupAsync(playerName);

            if (_settings.Value.NeedSaving)
            {
                _settings.Value.NeedSaving = false;
                _settings.Save();
            }

            return result;
        }

        /// <summary>
        /// This method avoids entering locks.
        /// Fast method does not perform logsearch and will work reliably only if any async request is completed earlier.
        /// Use pattern of calling async Get method during module init, then fast requests during runtime.
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public static string GetCurrentServerForPlayerFast(string playerName)
        {
            return _settings.Value.GetStatusFast(playerName);
        }

        public static async Task<WurmServer.ServerInfo.ServerGroup> GetServerGroupForPlayerAsync(string playerName)
        {
            var serverName = await GetCurrentServerForPlayerAsync(playerName);
            return WurmServer.ServerInfo.GetServerGroup(serverName);
        }

        /// <summary>
        /// This method avoids entering locks.
        /// Fast method does not perform logsearch and will work reliably only if any async request is completed earlier.
        /// Use pattern of calling async Get method during module init, then fast requests during runtime.
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public static WurmServer.ServerInfo.ServerGroup GetServerGroupForPlayerFast(string playerName)
        {
            var serverName = GetCurrentServerForPlayerFast(playerName);
            return WurmServer.ServerInfo.GetServerGroup(serverName);
        }

        /// <summary>
        /// Null if there is no data for this group
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static async Task<string> GetServerNameForGroup(string playerName, WurmServer.ServerInfo.ServerGroup group)
        {
            await _initializedTcs.Task;

            var result = await _settings.Value.GetStatusForServerGroupAsync(playerName, group);

            if (_settings.Value.NeedSaving)
            {
                _settings.Value.NeedSaving = false;
                _settings.Save();
            }

            return result;
        }

        public static async void ProcessLogEvent(string playerName, string logEvent)
        {
            await _initializedTcs.Task;

            var result = MatchServerFromLogLine(logEvent);
            if (result != null)
            {
                _settings.Value.SetStatus(playerName, result);
                _settings.Save();
            }
        }

        public static string MatchServerFromLogLine(string logEvent)
        {
            if (logEvent.Contains("You are on"))
            {
                Match match = Regex.Match(logEvent, @"\d+ other players are online.*\. You are on (.+) \(",
                    RegexOptions.Compiled);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
                else return null;
            }
            else return null;
        }
    }
}
