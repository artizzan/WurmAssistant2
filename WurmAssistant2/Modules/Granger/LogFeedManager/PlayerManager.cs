using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aldurcraft.Utility;
using Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger;
using Aldurcraft.WurmOnline.WurmLogsManager;
using Aldurcraft.WurmOnline.WurmLogsManager.Searcher;
using Aldurcraft.WurmOnline.WurmState;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger
{
    class PlayerManager
    {
        private readonly ModuleGranger _parentModule;
        private GrangerContext _context;

        //ManualServerGroupManager SGManager;
        HorseUpdatesManager HorseUpdateManager;

        public string PlayerName { get; private set; }

        private float _ahFreedomSkill;
        public float AhFreedomSkill
        {
            get { return _ahFreedomSkill; }
            set
            {
                _ahFreedomSkill = value;
                _parentModule.Settings.Value.SetAHSkill(
                    new LogFeedManager.CachedAHSkillID(WurmServer.ServerInfo.ServerGroup.Freedom, PlayerName),
                    value);
                _parentModule.Settings.DelayedSave();
            }
        }

        private float _ahEpicSkill;
        public float AhEpicSkill
        {
            get { return _ahEpicSkill; }
            set
            {
                _ahEpicSkill = value;
                _parentModule.Settings.Value.SetAHSkill(
                    new LogFeedManager.CachedAHSkillID(WurmServer.ServerInfo.ServerGroup.Epic, PlayerName),
                    value);
                _parentModule.Settings.DelayedSave();
            }
        }

        bool _skillObtainedFlag = false;

        public PlayerManager(ModuleGranger parentModule, GrangerContext context, string playerName)
        {
            this._parentModule = parentModule;
            this._context = context;
            this.PlayerName = playerName;

            //SGManager = new ManualServerGroupManager(PlayerName);
            HorseUpdateManager = new HorseUpdatesManager(_parentModule, _context, this);

            InitSkill();

            WurmLogs.SubscribeToLogFeed(PlayerName, new EventHandler<NewLogEntriesEventArgs>(OnNewLogEvents));
        }

        public void Update()
        {
            HorseUpdateManager.Update();
        }

        async Task InitSkill()
        {
            try
            {
                var sgSearchTask = PlayerServerTracker.GetServerGroupForPlayerAsync(PlayerName);

                var result = await WurmLogSearcherAPI.GetSkillsForPlayerAsync(
                    PlayerName,
                    10,
                    "Animal husbandry");

                AhFreedomSkill = await ObtainAhSkill(WurmServer.ServerInfo.ServerGroup.Freedom, result);
                AhEpicSkill = await ObtainAhSkill(WurmServer.ServerInfo.ServerGroup.Epic, result);

                // WU hack:
                AhFreedomSkill = await ObtainAhSkill(WurmServer.ServerInfo.ServerGroup.Unknown, result);

                // we don't need result, but we do need PlayerServerTracker to establish it before this is ready to process anything
                var sgResult = await sgSearchTask;

                _skillObtainedFlag = true;

                var eh = SkillObtained;
                if (eh != null) eh(this, new LogFeedManager.SkillObtainedEventArgs(PlayerName));
            }
            catch (Exception _e)
            {
                Logger.LogError("Something went wrong while trying to get AH skill for " + PlayerName, this, _e);
            }
        }

        private async Task<float> ObtainAhSkill(WurmServer.ServerInfo.ServerGroup serverGroup, Dictionary<WurmServer.ServerInfo.ServerGroup, float> searchResults)
        {
            var skillId = new LogFeedManager.CachedAHSkillID(serverGroup, PlayerName);
            float result = 0;
            // first try to find result in the last X days
            if (searchResults.TryGetValue(serverGroup, out result) == false)
            {
                // if nothing, try get a value saved in settings
                var obtained = _parentModule.Settings.Value.TryGetAHSkill(skillId, out result);
                if (!obtained || result == 0)
                {
                    // obtained value of 0 may indicate that longer search is needed
                    // get the last check date and push another search from that date
                    DateTime checkDate = DateTime.MinValue;
                    _parentModule.Settings.Value.TryGetAHCheckDate(skillId, out checkDate);
                    if (checkDate < DateTime.Now - TimeSpan.FromDays(10))
                    {
                        // if indeed last check was late, we need to search since then
                        var daysToSearch = (int)((DateTime.Now - checkDate).TotalDays);
                        var resultLongSearch = await WurmLogSearcherAPI.GetSkillsForPlayerAsync(
                            PlayerName,
                            daysToSearch > 365 ? 356 : daysToSearch,
                            "Animal husbandry");
                        resultLongSearch.TryGetValue(serverGroup, out result);
                    }
                    else
                    {
                        // this is weird, no result but check date is not so long ago
                        // maybe settings bugged? lets do a half year search just to be safe!
                        // note: this can trigger for server groups player doesn't care about
                        const int daysToSearch = 180;
                        var resultLongSearch = await WurmLogSearcherAPI.GetSkillsForPlayerAsync(
                            PlayerName,
                            daysToSearch,
                            "Animal husbandry");
                        resultLongSearch.TryGetValue(serverGroup, out result);
                    }
                }
            }
            _parentModule.Settings.Value.SetAHCheckDate(skillId, DateTime.Now);

            return result;
        }

        /// <summary>
        /// returns null if no skill data available yet
        /// </summary>
        /// <param name="serverGroup"></param>
        /// <returns></returns>
        public float? GetAhSkill(WurmServer.ServerInfo.ServerGroup serverGroup)
        {
            if (!_skillObtainedFlag) return null;

            if (serverGroup == WurmServer.ServerInfo.ServerGroup.Epic)
                return AhEpicSkill;
            else if (serverGroup == WurmServer.ServerInfo.ServerGroup.Freedom)
                return AhFreedomSkill;
            else
            {
                Logger.LogDebug("unknown server group for ah skill request, returning null, player: " + PlayerName, this);
                // WU hack:
                return AhFreedomSkill;
            }
        }

        public WurmServer.ServerInfo.ServerGroup GetCurrentServerGroup()
        {
            //return SGManager.CurrentGroup;

            //this method can return unknown if Servergroup is not yet established
            //we asume that such establishing is awaited properly at player manager init,
            //and no code relying on server group is actually executing until sg is known!
            return PlayerServerTracker.GetServerGroupForPlayerFast(PlayerName);
        }

        public event EventHandler<LogFeedManager.SkillObtainedEventArgs> SkillObtained;

        private void OnNewLogEvents(object sender, NewLogEntriesEventArgs e)
        {
            if (e.Entries.PlayerName == PlayerName)
            {
                foreach (var container in e.Entries.AllEntries)
                {
                    if (container.LogType == GameLogTypes.Event)
                    {
                        foreach (var entry in container.Entries)
                        {
                            //SGManager.UpdateCurrentGroupIfNeeded(entry);
                            if (_parentModule.Settings.Value.LogCaptureEnabled)
                            {
                                HorseUpdateManager.ProcessEventForHorseUpdates(entry);
                            }
                        }
                    }
                    else if (container.LogType == GameLogTypes.Skills)
                    {
                        foreach (var entry in container.Entries)
                        {
                            if (entry.StartsWith("Animal husbandry increased", StringComparison.Ordinal))
                            {
                                UpdateSkill(GeneralHelper.ExtractSkillLEVELFromLine(entry));
                            }
                            else if (entry.StartsWith("Animal husbandry decreased", StringComparison.Ordinal))
                            {
                                UpdateSkill(GeneralHelper.ExtractSkillLEVELFromLine(entry));
                            }
                        }
                    }
                }
            }
        }

        private void UpdateSkill(float skillLevel)
        {
            var currentServerGroup = GetCurrentServerGroup();
            if (currentServerGroup == WurmServer.ServerInfo.ServerGroup.Freedom)
            {
                AhFreedomSkill = skillLevel;
            }
            else if (currentServerGroup == WurmServer.ServerInfo.ServerGroup.Epic)
            {
                AhEpicSkill = skillLevel;
            }
            // WU hack:
            else if (currentServerGroup == WurmServer.ServerInfo.ServerGroup.Unknown)
            {
                AhFreedomSkill = skillLevel;
            }
        }

        public void Dispose()
        {
            WurmLogs.UnsubscribeFromLogFeed(PlayerName, OnNewLogEvents);
            //no need to clear SGManager becase it's using manual version
        }

        /// <summary>
        /// returns null if ah skill not yet found or server group not established
        /// </summary>
        /// <returns></returns>
        internal float? GetAhSkill()
        {
            return GetAhSkill(GetCurrentServerGroup());
        }
    }
}
