using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Ex;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Aldurcraft.Utility.MessageSystem;
using Aldurcraft.Utility.Notifier;
using Aldurcraft.Utility.PopupNotify;
using Aldurcraft.Utility.SoundEngine;
using Aldurcraft.Utility;
using Aldurcraft.WurmOnline.WurmLogsManager;
using DbLinq.Schema.Dbml;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    [DataContract]
    public abstract class TriggerBase : TriggerAbstract, ITrigger
    {
        [DataMember]
        private INotifier _sound;
        [DataMember]
        private INotifier _popup;
        [DataMember]
        private INotifier _message;
        [DataMember]
        private string _name;
        [DataMember]
        private TimeSpan _cooldown;
        [DataMember]
        private DateTime _cooldownUntil;
        [DataMember]
        private bool _cooldownEnabled;
        [DataMember]
        private bool _active;
        [DataMember]
        private HashSet<GameLogTypes> _logTypes;

        [DataMember]
        private bool _resetOnConditonHit;
        [DataMember]
        private bool _delayEnabled;
        [DataMember]
        private TimeSpan _delay;

        public bool DelayEnabled
        {
            get { return _delayEnabled; }
            set
            {
                _delayEnabled = value;
            }
        }

        public TimeSpan Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        public Func<bool> MuteChecker { private get; set; }

        private bool Muted
        {
            get
            {
                return MuteChecker();
            }
        }

        public IEnumerable<GameLogTypes> LogTypes { get { return _logTypes; } }

        public void AddLogType(GameLogTypes type)
        {
            if (LogTypesLocked) throw new TriggerException("child has blocked adding log types to this trigger");
            _logTypes.Add(type);
        }

        public void RemoveLogType(GameLogTypes type)
        {
            _logTypes.Remove(type);
        }

        public virtual bool CheckLogType(GameLogTypes type)
        {
            return _logTypes.Contains(type);
        }

        public virtual INotifier Sound
        {
            get { return _sound; }
            set
            {
                if (value != null && !(value is ISoundNotifier)) throw new TriggerException("invalid type, must be ISoundNotifier");
                _sound = value;
            }
        }

        public virtual INotifier Popup
        {
            get { return _popup; }
            set
            {
                if (value != null && !(value is IPopupNotifier)) throw new TriggerException("invalid type, must be IPopupNotifier");
                _popup = value;
            }
        }

        public virtual INotifier Message
        {
            get { return _message; }
            set
            {
                if (value != null && !(value is IMessageNotifier)) throw new TriggerException("invalid type, must be IMessageNotifier");
                _message = value;
            }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        protected DateTime CooldownUntil
        {
            get { return _cooldownUntil; }
            set { _cooldownUntil = value; }
        }

        public bool CooldownEnabled
        {
            get { return _cooldownEnabled; }
            set
            {
                _cooldownEnabled = value;
                if (!value) CooldownUntil = DateTime.MinValue;
            }
        }

        public TimeSpan Cooldown
        {
            get { return _cooldown; }
            set { _cooldown = value; }
        }

        public bool Active
        {
            get { return _active; }
            set { _active = value; }
        }

        protected TriggerBase()
        {
            ScheduledNotify = null;
            Init();
        }

        void Init()
        {
            _logTypes = new HashSet<GameLogTypes>();
            MuteChecker = () => false;
            Active = true;
        }

        [OnDeserializing]
        void OnDes(StreamingContext context)
        {
            Init();
        }

        public virtual void Update(string logMessage, DateTime dateTimeNow)
        {
            if (Active)
            {
                if (!CooldownEnabled)
                {
                    if (CheckCondition(logMessage))
                    {
                        DoNotifies(dateTimeNow);
                    }
                }
                else
                {
                    if (ResetOnConditonHit)
                    {
                        if (CheckCondition(logMessage))
                        {
                            if (dateTimeNow > CooldownUntil) DoNotifies(dateTimeNow);
                            CooldownUntil = dateTimeNow + Cooldown;
                        }
                    }
                    else if (dateTimeNow > CooldownUntil)
                    {
                        if (CheckCondition(logMessage))
                        {
                            DoNotifies(dateTimeNow);
                            CooldownUntil = dateTimeNow + Cooldown;
                        }
                    }
                }
            }
        }

        private DateTime? ScheduledNotify { get; set; }

        public virtual bool DefaultDelayFunctionalityDisabled { get { return false; } }

        public virtual void FixedUpdate(DateTime dateTimeNow)
        {
            if (ScheduledNotify != null)
            {
                if (dateTimeNow > ScheduledNotify)
                {
                    ScheduledNotify = null;
                    FireAllNotification();
                }
            }
        }

        protected virtual void DoNotifies(DateTime dateTimeNow)
        {
            var fireNotifyOn = dateTimeNow;
            if (DelayEnabled)
            {
                fireNotifyOn = fireNotifyOn + Delay;
            }
            ScheduledNotify = fireNotifyOn;
        }

        protected void FireAllNotification()
        {
            if (Sound != null && !Muted) Sound.Notify();
            if (Popup != null) Popup.Notify();
            if (Message != null) Message.Notify();
        }

        protected abstract bool CheckCondition(string logMessage); 

        public virtual IEnumerable<INotifier> GetNotifiers()
        {
            var notifiers = new List<INotifier>();
            if (Sound != null) notifiers.Add(Sound);
            if (Popup != null) notifiers.Add(Popup);
            if (Message != null) notifiers.Add(Message);
            return notifiers;
        }

        public virtual void AddNotifier(INotifier notifier)
        {
            if (Sound == null)
            {
                if (notifier is ISoundNotifier)
                {
                    Sound = notifier;
                    return;
                }
            }
            if (Popup == null)
            {
                if (notifier is IPopupNotifier)
                {
                    Popup = notifier;
                    return;
                }
            }
            if (Message == null)
            {
                if (notifier is IMessageNotifier)
                {
                    Message = notifier;
                    return;
                }
            }
            throw new TriggerException(string.Format("could not add notifier of type {0}, either this type exists in trigger or is not supported", notifier.GetType()));
        }

        public virtual void RemoveNotifier(INotifier notifier)
        {
            if (notifier is ISoundNotifier)
            {
                Sound = null;
                return;
            }
            if (notifier is IPopupNotifier)
            {
                Popup = null;
                return;
            }
            if (notifier is IMessageNotifier)
            {
                Message = null;
                return;
            }
            throw new TriggerException("Removal of notifier failed, argument exact type: " + notifier.GetType());
        }

        public virtual string LogTypesAspect
        {
            get { return string.Join(", ", _logTypes); }
        }

        public virtual string ConditionAspect { get { return "Unknown"; } }

        public virtual string TypeAspect { get { return "Unknown"; } }

        public ThreeStateBool HasSoundAspect
        {
            get
            {
                if (Sound != null)
                {
                    if (Sound.HasEmptySound)
                    {
                        return ThreeStateBool.Error;
                    }
                    else
                    {
                        return ThreeStateBool.True;
                    }
                }
                return ThreeStateBool.False;
            }
        }

        public ThreeStateBool HasPopupAspect
        {
            get
            {
                if (Popup != null)
                {
                    return ThreeStateBool.True;
                }
                return ThreeStateBool.False;
            }
        }

        public virtual string CooldownRemainingAspect
        {
            get
            {
                if (!CooldownEnabled) return string.Empty;
                var cdRem = CooldownUntil - DateTime.Now;
                if (cdRem.Ticks > 0) return cdRem.ToFriendlyStringEx();
                return string.Empty;
            }
        }

        protected GameLogTypes[] LockedLogTypes
        {
            set
            {
                foreach (var gameLogType in value)
                {
                    AddLogType(gameLogType);
                }
                LogTypesLocked = true;
            }
        }

        public bool LogTypesLocked { get; private set; }

        public virtual IEnumerable<ITriggerConfig> Configs
        {
            get
            {
                return new[] { new TriggerBaseConfig(this) };
            }
        }

        private EditTrigger _currentEditUi = null;
        public EditTrigger ShowAndGetEditUi(Form parent) 
        {
            try
            {
                if (_currentEditUi == null)
                {
                    _currentEditUi = new EditTrigger(this);
                    _currentEditUi.ShowCenteredEx(parent);
                }
                else
                {
                    _currentEditUi.ShowThisDarnWindowDammitEx();
                }
                return _currentEditUi;
            }
            catch (ObjectDisposedException)
            {
                _currentEditUi = null;
                return ShowAndGetEditUi(parent);
            }
        }

        public bool ResetOnConditonHit
        {
            get { return _resetOnConditonHit; }
            set { _resetOnConditonHit = value; }
        }

        internal void SetLogType(GameLogTypes logType, System.Windows.Forms.CheckState checkState)
        {
            if (checkState == CheckState.Checked)
            {
                AddLogType(logType);
            }
            else
            {
                RemoveLogType(logType);
            }
        }
    }
}
