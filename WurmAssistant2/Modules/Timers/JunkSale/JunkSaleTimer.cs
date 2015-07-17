using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Aldurcraft.WurmOnline.WurmLogsManager;
using Aldurcraft.WurmOnline.WurmLogsManager.Searcher;
using Aldurcraft.WurmOnline.WurmState;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Timers
{
    public class JunkSaleTimer : WurmTimer
    {
        [DataContract]
        public class JunkSaleTimerSettings
        {
            [DataMember]
            public DateTime CooldownUntil;
            [DataMember]
            public int CurrentTotalAmount;

            public JunkSaleTimerSettings()
            {
                InitMe();
            }

            [OnDeserializing]
            private void OnDes(StreamingContext context)
            {
                InitMe();
            }

            private void InitMe()
            {
                CooldownUntil = DateTime.MinValue;
            }
        }

        PersistentObject<JunkSaleTimerSettings> Settings;

        public override void Initialize(PlayerTimersGroup parentGroup, string player, string timerId,
            WurmServer.ServerInfo.ServerGroup serverGroup, string compactId)
        {
            base.Initialize(parentGroup, player, timerId, serverGroup, compactId);
            Settings = new PersistentObject<JunkSaleTimerSettings>(new JunkSaleTimerSettings());
            Settings.SetFilePathAndLoad(SettingsSavePath);
            TimerDisplay.ShowSkill = true;
            VerifyMoneyAmountAgainstCd();
            UpdateMoneyCounter();
            InitCompleted = true;
        }

        public override void Update(bool engineSleeping)
        {
            base.Update(engineSleeping);
            Settings.Update();
            if (TimerDisplay.Visible) TimerDisplay.UpdateCooldown(Settings.Value.CooldownUntil - DateTime.Now);
            VerifyMoneyAmountAgainstCd();
        }

        void VerifyMoneyAmountAgainstCd()
        {
            if (Settings.Value.CurrentTotalAmount != 0 && DateTime.Now > Settings.Value.CooldownUntil)
            {
                Settings.Value.CurrentTotalAmount = 0;
                UpdateMoneyCounter();
            }
        }

        public override void Stop()
        {
            //cleanup here
            Settings.Save();
            base.Stop();
        }

        public override void HandleNewEventLogLine(string line)
        {
            if (line.StartsWith("You receive", StringComparison.Ordinal))
            {
                Match match = Regex.Match(line, @"You receive (\d+) irons\.");
                if (match.Success)
                {
                    if (DateTime.Now > Settings.Value.CooldownUntil)
                    {
                        // reset the timer only if its already over
                        Settings.Value.CooldownUntil = DateTime.Now + TimeSpan.FromHours(1);
                        Settings.Value.CurrentTotalAmount = 0;
                    }

                    try
                    {
                        Settings.Value.CurrentTotalAmount += int.Parse(match.Groups[1].Value);
                    }
                    catch (FormatException _e)
                    {
                        Logger.LogError("Invalid format while attempting to parse junksale gain amount", this, _e);
                    }

                    UpdateMoneyCounter();
                }
            }
        }

        private void UpdateMoneyCounter()
        {
            TimerDisplay.SetCustomStringAsSkill(PrepareStrDisplayForMoneyAmount(Settings.Value.CurrentTotalAmount));
        }

        string PrepareStrDisplayForMoneyAmount(int amount)
        {
            int coppers = amount/100;
            int irons = amount%100;
            return string.Format("{0}c{1}i", coppers, irons);
        }
    }
}
