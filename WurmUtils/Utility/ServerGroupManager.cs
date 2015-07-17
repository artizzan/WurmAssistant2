using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aldurcraft.WurmOnline.WurmState;
using Aldurcraft.WurmOnline.WurmLogsManager;
using Aldurcraft.WurmOnline.WurmLogsManager.Searcher;
using Aldurcraft.Utility;
using System.Threading.Tasks;

namespace Aldurcraft.WurmOnline.Utility
{
    // this is obsolete in favor of PlayerServerTracker system
    [Obsolete]
    public class ServerGroupManager
    {
        public class ServerGroupEventArgs
        {
            public readonly WurmServer.ServerInfo.ServerGroup CurrentGroup;
            public ServerGroupEventArgs(WurmServer.ServerInfo.ServerGroup currentGroup)
            {
                this.CurrentGroup = currentGroup;
            }
        }

        public string PlayerName { get; private set; } 
        public WurmServer.ServerInfo.ServerGroup CurrentGroup { get; private set; } 

        /// <summary>
        /// Call this constructor from derived classes only,
        /// this class is not fully functional
        /// </summary>
        /// <param name="playerName"></param>
        protected ServerGroupManager(string playerName)
        {
            PlayerName = playerName;
            CurrentGroup = WurmServer.ServerInfo.ServerGroup.Unknown;

            Initialize();
        }

        async Task Initialize()
        {
            try
            {
                LogSearchData lgs = new LogSearchData();
                lgs.SetSearchCriteria(
                    PlayerName,
                    GameLogTypes.Event,
                    DateTime.Now - TimeSpan.FromDays(5),
                    DateTime.Now,
                    "",
                    SearchTypes.RegexEscapedCaseIns);

                lgs = await WurmLogSearcherAPI.SearchWurmLogsAsync(lgs);

                foreach (string line in lgs.AllLines)
                {
                    ProcessLogLine(line, false);
                }
 
                ServerGroupEstablished = true;
            }
            catch (Exception _e)
            {
                Logger.LogError("Something went wrong when trying to establish server group for player: " + PlayerName, this, _e);
            }
        }

        /// <summary>
        /// This flag becomes true when either log search finishes or live logs find the group,
        /// it is used to prevent log search overwriting live log results, 
        /// which are guaranteed to be more accurate.
        /// </summary>
        bool ServerGroupEstablished = false;

        protected void ProcessLogLine(string line, bool liveLogs)
        {
            if (line.Contains("You are on"))
            {
                string serverName; //not actually used
                WurmServer.ServerInfo.ServerGroup group = WurmLogSearcherAPI.GetServerGroupFromLine(line, out serverName);
                if (group != WurmServer.ServerInfo.ServerGroup.Unknown)
                {
                    if (liveLogs)
                    {
                        CurrentGroup = group;
                        ServerGroupEstablished = true;
                    }
                    else
                    {
                        // basically if live logs found group, do not change it using log search results
                        // this should prevent a race
                        if (!ServerGroupEstablished)
                            CurrentGroup = group;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// obtains and keeps track of server group specified player is currently playing on,
    /// this version requires manual feeding of all log events
    /// </summary>
    [Obsolete]
    public class ManualServerGroupManager : ServerGroupManager
    {
        public ManualServerGroupManager(string playerName)
            : base(playerName) 
        {
        }

        /// <summary>
        /// update by processing log messages, send here lines without timestamp as provided by WurmLogs events
        /// </summary>
        /// <param name="line">log line</param>
        public void UpdateCurrentGroupIfNeeded(string line)
        {
            ProcessLogLine(line, true);
        }
    }

    /// <summary>
    /// obtains and keeps track of server group specified player is currently playing on,
    /// this version is independent and manages log feed on its own
    /// </summary>
    [Obsolete]
    public class IndependentServerGroupManager : ServerGroupManager, IDisposable
    {
        public IndependentServerGroupManager(string playerName) 
            : base(playerName) 
        {
            // base constructor calls Initialize(), but because its async method, 
            // it continues to here as soon as Initialize hits a search block
            WurmLogs.SubscribeToLogFeed(PlayerName, HandleLogEvents);
            // so we subscribe to log feed immediatelly and probably won't ever miss event message
            // C# rocks!
        }

        void HandleLogEvents(object sender, NewLogEntriesEventArgs e)
        {
            if (e.Entries.PlayerName == PlayerName)
            {
                foreach (var container in e.Entries.AllEntries)
                {
                    if (container.LogType == GameLogTypes.Event)
                    {
                        foreach (var entry in container.Entries)
                        {
                            ProcessLogLine(entry, true);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            WurmLogs.UnsubscribeFromLogFeed(PlayerName, HandleLogEvents);
        }
    }
}
