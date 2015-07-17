using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Aldurcraft.WurmOnline.WurmLogsManager;
using Aldurcraft.Utility.Notifier;
using System.Windows.Forms;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Triggers
{
    public enum ThreeStateBool { True, False, Error }

    public interface ITrigger
    {
        void AddLogType(GameLogTypes type);
        void RemoveLogType(GameLogTypes type);
        /// <summary>
        /// Is log type monitored by this trigger
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool CheckLogType(GameLogTypes type);
        /// <summary>
        /// Delegate to check if trigger holder is in muted state
        /// </summary>
        Func<bool> MuteChecker { set; } 
        string Name { get; set; }

        /// <summary>
        /// Happens in a constant loop
        /// </summary>
        /// <param name="dateTimeNow"></param>
        void FixedUpdate(DateTime dateTimeNow);
        /// <summary>
        /// Happens when new log messages arrive, when matching log types
        /// </summary>
        /// <param name="logMessage"></param>
        /// <param name="dateTimeNow"></param>
        void Update(string logMessage, DateTime dateTimeNow);

        IEnumerable<INotifier> GetNotifiers();

        TimeSpan Cooldown{ get; set; }
        bool Active { get; set; }

        void AddNotifier(INotifier notifier);
        void RemoveNotifier(INotifier notifier);

        string LogTypesAspect { get; }
        string ConditionAspect { get; }
        string TypeAspect { get; }
        string CooldownRemainingAspect { get; }
        ThreeStateBool HasSoundAspect { get; }
        ThreeStateBool HasPopupAspect { get; }

        /// <summary>
        /// Should log type user choice be disabled for this trigger
        /// </summary>
        bool LogTypesLocked { get; }
        IEnumerable<ITriggerConfig> Configs { get; }

        EditTrigger ShowAndGetEditUi(Form parent);
    }

    public interface ITriggerConfig
    {
        UserControl ControlHandle { get; }
    }
}
