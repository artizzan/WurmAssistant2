using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Aldurcraft.WurmOnline.WurmLogsManager;
using Aldurcraft.WurmOnline.WurmLogsManager.Searcher;
using Aldurcraft.WurmOnline.WurmState;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Timers
{
    public class SermonTimer : WurmTimer
    {
        private static readonly TimeSpan SermonPreacherCooldown = new TimeSpan(3, 0, 0);

        DateTime _dateOfNextSermon = DateTime.MinValue;
        DateTime DateOfNextSermon
        {
            get { return _dateOfNextSermon; }
            set {
                _dateOfNextSermon = value; 
                CDNotify.CooldownTo = value; 
            }
        }

        //DateTime CooldownResetSince = DateTime.MinValue;

        public override void Initialize(PlayerTimersGroup parentGroup, string player, string timerId, WurmServer.ServerInfo.ServerGroup serverGroup, string compactId)
        {
            base.Initialize(parentGroup, player, timerId, serverGroup, compactId);
            TimerDisplay.SetCooldown(SermonPreacherCooldown);

            PerformAsyncInits();
        }

        async Task PerformAsyncInits()
        {
            try
            {
                List<string> lines = await GetLogLinesFromLogHistoryAsync(GameLogTypes.Event, TimeSpan.FromDays(2));

                string lastSermonLine = null;
                foreach (string line in lines)
                {
                    if (line.Contains("You finish this sermon"))
                    {
                        lastSermonLine = line;
                    }
                }
                if (lastSermonLine != null)
                {
                    UpdateDateOfNextSermon(lastSermonLine, false);
                }

                InitCompleted = true;
            }
            catch (Exception exception)
            {
                Logger.LogError("init error", this, exception);
            }
        }

        public override void Update(bool engineSleeping)
        {
            base.Update(engineSleeping);
            if (TimerDisplay.Visible) TimerDisplay.UpdateCooldown(DateOfNextSermon);
        }

        protected override void HandleServerChange()
        {
            //UpdateDateOfLastCooldownReset();
        }

        public override void HandleNewEventLogLine(string line)
        {
            if (line.StartsWith("You finish this sermon", StringComparison.Ordinal))
            {
                UpdateDateOfNextSermon(line, true);
            }
        }

        void UpdateDateOfNextSermon(string line, bool liveLogs)
        {
            //UpdateDateOfLastCooldownReset();
            DateTime dateOfThisLine;
            if (liveLogs)
            {
                dateOfThisLine = DateTime.Now;
                DateOfNextSermon = dateOfThisLine + SermonPreacherCooldown;
            }
            else
            {
                if (WurmLogSearcherAPI.TryParseDateTimeFromSearchResultLine(line, out dateOfThisLine))
                {
                    DateOfNextSermon = dateOfThisLine + SermonPreacherCooldown;
                }
                else
                {
                    //do nothing, whatever happened
                    Logger.LogError("date parse error", this);
                }
            }

            //if (DateOfNextSermon > CooldownResetSince + TimeSpan.FromDays(1))
            //{
            //    DateOfNextSermon = CooldownResetSince + TimeSpan.FromDays(1);
            //}
        }

        //void UpdateDateOfLastCooldownReset()
        //{
        //    var result = GetLatestUptimeCooldownResetDate();
        //    if (result > DateTime.MinValue) CooldownResetSince = result;
        //}
    }
}
