using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Timers
{
    public class TestTimer : WurmTimer
    {
        TimeSpan CD_duration = TimeSpan.FromSeconds(10);

        public override void Initialize(PlayerTimersGroup parentGroup, string player, string timerId, Aldurcraft.WurmOnline.WurmState.WurmServer.ServerInfo.ServerGroup serverGroup, string compactId)
        {
            base.Initialize(parentGroup, player, timerId, serverGroup, compactId);
            TimerDisplay.SetCooldown(CD_duration);
            this.InitCompleted = true;
        }

        public override void Stop()
        {
            base.Stop();
        }

        public override void HandleNewEventLogLine(string line)
        {
            if (line.StartsWith("A storage bin", StringComparison.Ordinal))
            {
                Logger.LogDebug("A storage bin indeed!", this);
                CDNotify.CooldownTo = DateTime.Now + CD_duration;
            }
        }

        public override void Update(bool engineSleeping)
        {
            base.Update(engineSleeping);
            CDNotify.Update(engineSleeping);
            if (TimerDisplay.Visible)
            {
                //LoggingEngine.Logger.LogDebug("Updated ui!", this);
                TimerDisplay.UpdateCooldown(CDNotify.CooldownTo - DateTime.Now);
            }
        }
    }
}
