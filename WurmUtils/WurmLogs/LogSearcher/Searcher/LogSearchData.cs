using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Aldurcraft.WurmOnline.WurmLogsManager.Searcher
{
    //note: this is no needed used outside this assembly
    internal enum LogSearchDataIDs
    {
        Unspecified,
        TimersMeditSkill,
        TimersMeditHistory,
        LogSearcherForceRecache,
        TimingAssistUptimeSearch,
        TimingAssistDateTimeSearch,
        TimersMeditPathAdvance,
        TimersFaithSkill,
        TimersPrayHistory,
        TimersSermonLookup,
        TimersAlignmentLookup,
        TimersLockpickingHistory,
        GrangerAHLookup
    }

    public class LogSearchData
    {
        internal class SearchData
        {
            internal string Player;
            internal GameLogTypes GameLogType;
            internal DateTime TimeFrom;
            internal DateTime TimeTo;
            internal string SearchKey;
            internal SearchTypes SearchType;

            /// <summary>
            /// In case this is PM-log search criteria, this should be the PM recipient name.
            /// Not supplying this value will result in a general PM search trough all conversations.
            /// </summary>
            internal string PM_Player = null;

            /// <summary>
            /// Holds search criteria for scheduling log searches
            /// </summary>
            /// <param name="player"></param>
            /// <param name="gamelogtype"></param>
            /// <param name="timefrom"></param>
            /// <param name="timeto"></param>
            /// <param name="searchkey">null or empty string to indicate this requires no match search</param>
            /// <param name="searchtype"></param>
            internal SearchData(string player, GameLogTypes gamelogtype, DateTime timefrom, DateTime timeto, string searchkey, SearchTypes searchtype)
            {
                this.Player = player;
                this.GameLogType = gamelogtype;
                this.TimeFrom = timefrom;
                this.TimeTo = timeto;
                if (searchkey != null) this.SearchKey = searchkey;
                else this.SearchKey = "";
                this.SearchType = searchtype;
            }
        }

        internal struct SingleSearchMatch
        {
            internal long Begin;
            internal long Length;
            internal DateTime MatchDate;

            internal SingleSearchMatch(long begin, long end, DateTime matchdate)
            {
                this.Begin = begin;
                this.Length = end;
                this.MatchDate = matchdate;
            }
        }

        /// <summary>
        /// Can be used to cancel this search if it's not yet completed
        /// </summary>
        public volatile bool StopSearching = false;

        /// <summary>
        /// Leave null if using Async search requests
        /// </summary>
        internal Control CallerControl;
        /// <summary>
        /// Leave unset if using Async search requests
        /// </summary>
        internal LogSearchDataIDs CallbackID = LogSearchDataIDs.Unspecified;

        /// <summary>
        /// Holds all log lines for specified criteria, do not modify before search is completed!
        /// </summary>
        public List<string> AllLines = new List<string>();
        //public List<long> AllLinesCharStartIndex = new List<long>();
        internal string[] AllLinesArray = null;

        /// <summary>
        /// Holds all search matches for specified criteria, do not modify before search is completed!
        /// </summary>
        internal List<SingleSearchMatch> SearchResults = new List<SingleSearchMatch>();
        internal SearchData SearchCriteria;
        int Count = 0;
        bool Ready = false;

        /// <summary>
        /// Set the search criteria
        /// </summary>
        /// <param name="player">Player name, case sensitive</param>
        /// <param name="gamelogtype">Type of the log</param>
        /// <param name="timefrom">Begin date boundary</param>
        /// <param name="timeto">End date boundary</param>
        /// <param name="searchkey">Text to search for, or regex expression for regex search type</param>
        /// <param name="searchtype">Search type</param>
        /// <param name="PM_recipient">
        /// In case this is PM-log search criteria, this should be the PM recipient name.
        /// Not supplying this value will result in a general PM search trough all conversations.
        /// </param>
        public void SetSearchCriteria(string player, GameLogTypes gamelogtype, DateTime timefrom, DateTime timeto, string searchkey, SearchTypes searchtype, string PM_recipient = null)
        {
            SearchCriteria = new SearchData(player, gamelogtype, timefrom, timeto, searchkey, searchtype);
            if (PM_recipient != null) this.SetPM_Player(PM_recipient);
        }

        /// <summary>
        /// Set PM recipient name to further narrow the search in case of PM logs
        /// </summary>
        /// <param name="player"></param>
        internal void SetPM_Player(string player)
        {
            player = player.Trim();
            if (player != "") this.SearchCriteria.PM_Player = player;
            else this.SearchCriteria.PM_Player = null;
        }

        internal void AddResult(int begin, int end, DateTime matchdate)
        {
            Count++;
            SearchResults.Add(new SingleSearchMatch(begin, end, matchdate));
        }

        internal void Finish()
        {
            Ready = true;
        }

        internal bool IsReady()
        {
            return Ready;
        }
    }
}
