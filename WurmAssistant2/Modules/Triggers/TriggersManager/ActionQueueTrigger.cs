using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Aldurcraft.Utility.MessageSystem;
using Aldurcraft.Utility.Notifier;
using Aldurcraft.Utility.PopupNotify;
using Aldurcraft.Utility.SoundEngine;
using Aldurcraft.Utility;
using Aldurcraft.WurmOnline.WurmLogsManager;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    // This class is a different beast entirely

    [DataContract]
    public class ActionQueueTrigger : TriggerBase
    {
        public ActionQueueTrigger()
        {
            Init();
        }

        private void Init()
        {
            LockedLogTypes = new[] { GameLogTypes.Event };
            _notificationDelay = 1.0D;
            _lastActionFinished = DateTime.Now;
            _lastActionStarted = DateTime.Now;
            _lastEventLine = string.Empty;
        }

        [OnDeserializing]
        private void OnDes(StreamingContext context)
        {
            Init();
        }

        [DataMember]
        private double _notificationDelay;

        public double NotificationDelay
        {
            get { return _notificationDelay; }
            set { _notificationDelay = value; }
        }

        private bool _levelingMode;
        private string _lastEventLine;
        bool _notificationScheduled;
        // last queue ending action
        DateTime _lastActionFinished;
        // last queue starting action
        DateTime _lastActionStarted;
        // wurm log line that triggered queue sound
        private string _logEntryThatTriggeredLastQueueSound;

        public override bool CheckLogType(GameLogTypes type)
        {
            return type == GameLogTypes.Event;
        }

        public override void Update(string logMessage, DateTime dateTimeNow)
        {
            bool _PlayerActionStarted = false;

            foreach (string cond in LogQueueParseHelper.ActionStart)
            {
                if (logMessage.StartsWith(cond, StringComparison.Ordinal))
                {
                    _PlayerActionStarted = true;
                    _levelingMode = false;
                }
            }
            foreach (string cond in LogQueueParseHelper.ActionStart_contains)
            {
                if (logMessage.Contains(cond))
                {
                    _PlayerActionStarted = true;
                    _levelingMode = false;
                }
            }

            if (_levelingMode)
            {
                foreach (string cond in LogQueueParseHelper.LevelingEnd)
                {
                    if (logMessage.StartsWith(cond, StringComparison.Ordinal))
                    {
                        _levelingMode = false;
                    }
                }
            }
            if (logMessage.StartsWith(LogQueueParseHelper.LevelingModeStart, StringComparison.Ordinal))
            {
                _levelingMode = true;
            }

            foreach (string cond in LogQueueParseHelper.ActionFalstart)
            {
                if (logMessage.StartsWith(cond, StringComparison.Ordinal)) _PlayerActionStarted = false;
            }
            if (_PlayerActionStarted == true)
            {
                _lastActionStarted = DateTime.Now;
            }

            bool _PlayerActionFinished = false;

            foreach (string cond in LogQueueParseHelper.ActionEnd)
            {
                if (logMessage.StartsWith(cond, StringComparison.Ordinal)) _PlayerActionFinished = true;
            }
            foreach (string cond in LogQueueParseHelper.ActionEnd_contains)
            {
                if (logMessage.Contains(cond))
                    _PlayerActionFinished = true;
            }
            foreach (string cond in LogQueueParseHelper.ActionFalsEnd)
            {
                if (logMessage.StartsWith(cond, StringComparison.Ordinal))
                    _PlayerActionFinished = false;
            }
            foreach (string cond in LogQueueParseHelper.ActionFalsEndDueToLastAction)
            {
                if (_lastEventLine.StartsWith(cond, StringComparison.Ordinal))
                    _PlayerActionFinished = false;
            }

            if (_levelingMode) _PlayerActionFinished = false;
            if (_PlayerActionFinished == true)
            {
                _logEntryThatTriggeredLastQueueSound = logMessage;
                _lastActionFinished = DateTime.Now;
                // if action finished, older action started is no longer valid
                // and should not disable queuesound in next conditional
                _lastActionStarted = _lastActionStarted.AddSeconds(-_notificationDelay); // datetime is not nullable
                _notificationScheduled = true;
            }

            // disable scheduled queue sound if new action started before its played
            if (_lastActionStarted.AddSeconds(_notificationDelay) >= DateTime.Now)
            {
                _notificationScheduled = false;
            }

            if (!Active) _notificationScheduled = false;

            _lastEventLine = logMessage;
        }

        public override void FixedUpdate(DateTime dateTimeNow)
        {
            if (Active && _notificationScheduled && dateTimeNow >= _lastActionFinished.AddSeconds(_notificationDelay))
            {
                if (!CooldownEnabled)
                {
                    DoNotifies(dateTimeNow);
                }
                else
                {
                    if (ResetOnConditonHit)
                    {
                        if (dateTimeNow > CooldownUntil)
                        {
                            DoNotifies(dateTimeNow);
                        }
                        else
                        {
                            CooldownUntil = dateTimeNow + Cooldown;
                            _notificationScheduled = false;
                        }
                    }
                    else
                    {
                        if (dateTimeNow > CooldownUntil)
                        {
                            DoNotifies(dateTimeNow);
                            CooldownUntil = dateTimeNow + Cooldown;
                        }
                    }
                }
            }
        }

        protected override void DoNotifies(DateTime dateTimeNow)
        {
            base.FireAllNotification();
            Logger.LogInfo("Queue notification triggered due to event: " + _logEntryThatTriggeredLastQueueSound);
            _notificationScheduled = false;
        }

        protected override bool CheckCondition(string logMessage)
        {
            throw new TriggerException("ActionQueueTrigger does not implement this method");
        }

        public override string ConditionAspect
        {
            get { return "# when action queue is finished #"; }
        }

        public override string TypeAspect
        {
            get { return "Action Queue"; }
        }

        public override IEnumerable<ITriggerConfig> Configs
        {
            get
            {
                return new List<ITriggerConfig>(base.Configs) { new ActionQueueTriggerConfig(this) };
            }
        }

        public override bool DefaultDelayFunctionalityDisabled
        {
            get { return true; }
        }
    }
}
