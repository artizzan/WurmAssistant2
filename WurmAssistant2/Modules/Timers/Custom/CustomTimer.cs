using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aldurcraft.WurmOnline.WurmLogsManager;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Timers
{
    public class CustomTimer : WurmTimer
    {
        WurmTimerDescriptors.CustomTimerOptions Options;

        DateTime _cooldownTo = DateTime.MinValue;
        DateTime CooldownTo
        {
            get { return _cooldownTo; }
            set
            {
                _cooldownTo = value;
                CDNotify.CooldownTo = value;
            }
        }
        DateTime UptimeResetSince = DateTime.MinValue;

        //happens before Initialize
        public void ApplyCustomTimerOptions(WurmTimerDescriptors.CustomTimerOptions options)
        {
            Options = options;
        }

        public override void Initialize(PlayerTimersGroup parentGroup, string player, string timerId, Aldurcraft.WurmOnline.WurmState.WurmServer.ServerInfo.ServerGroup serverGroup, string compactId)
        {
            base.Initialize(parentGroup, player, timerId, serverGroup, compactId);
            //MoreOptionsAvailable = true;
            TimerDisplay.SetCooldown(Options.Duration);

            PerformAsyncInits();
        }

        async Task PerformAsyncInits()
        {
            try
            {
                UpdateDateOfLastCooldownReset();

                HashSet<GameLogTypes> condLogTypes = new HashSet<GameLogTypes>(
                    Options.TriggerConditions.Select(x => x.LogType));

                foreach (var type in condLogTypes)
                {
                    GameLogTypes captType = type;
                    List<string> lines = await GetLogLinesFromLogHistoryAsync(captType,
                        DateTime.Now - Options.Duration - TimeSpan.FromDays(2));
                    foreach (var cond in Options.TriggerConditions)
                    {
                        if (cond.LogType == captType) ProcessLinesForCooldownTriggers(lines, cond, false);
                    }
                }

                InitCompleted = true;
            }
            catch (Exception _e)
            {
                Logger.LogError("init error", this, _e);
            }
        }

        public override void Update(bool engineSleeping)
        {
            base.Update(engineSleeping);
            if (TimerDisplay.Visible) TimerDisplay.UpdateCooldown(CooldownTo);
        }

        public override void Stop()
        {

            base.Stop();
        }

        public override void HandleAnyLogLine(NewLogEntriesContainer container)
        {
            foreach (var cond in Options.TriggerConditions)
            {
                if (cond.LogType == container.LogType)
                {
                    ProcessLinesForCooldownTriggers(container.Entries, cond, true);
                }
            }
        }

        public override void OpenMoreOptions(FormTimerSettingsDefault form)
        {
            base.OpenMoreOptions(form);
        }

        protected override void HandleServerChange()
        {
            UpdateDateOfLastCooldownReset();
            try
            {
                TriggerCooldown(CooldownTo - Options.Duration);
            }
            catch (Exception _e)
            {
                if (CooldownTo == DateTime.MinValue)
                {
                    TriggerCooldown(CooldownTo);
                }
                else
                {
                    Logger.LogInfo("unknown problem with HandleServerChange", this, _e);
                }
            }
        }

        void ProcessLinesForCooldownTriggers(List<string> lines, WurmTimerDescriptors.CustomTimerOptions.Condition condition, bool liveLogs)
        {
            foreach (string line in lines)
            {
                RegexOptions opt = new RegexOptions();
                if (!Options.IsRegex) opt = RegexOptions.IgnoreCase;
                if (Regex.IsMatch(line, condition.RegexPattern, opt))
                {
                    if (liveLogs)
                    {
                        TriggerCooldown(DateTime.Now);
                    }
                    else
                    {
                        DateTime startDate;
                        if (Aldurcraft.WurmOnline.WurmLogsManager.Searcher.WurmLogSearcherAPI.TryParseDateTimeFromSearchResultLine(line, out startDate))
                        {
                            TriggerCooldown(startDate);
                        }
                        else Logger.LogInfo("parse error, custom timer ID: " + this.TimerID + "; Line: " + line, this);
                    }
                }
            }
        }

        void TriggerCooldown(DateTime startDate)
        {
            if (Options.ResetOnUptime && startDate > UptimeResetSince)
            {
                DateTime cd_to = startDate + Options.Duration;
                DateTime NextUptimeReset = UptimeResetSince + TimeSpan.FromDays(1);
                if (cd_to > NextUptimeReset)
                    cd_to = NextUptimeReset;
                CooldownTo = cd_to;
            }
            else
            {
                CooldownTo = startDate + Options.Duration;
            }
        }

        void UpdateDateOfLastCooldownReset()
        {
            var result = GetLatestUptimeCooldownResetDate();
            if (result > DateTime.MinValue) UptimeResetSince = result;
        }
    }
}
