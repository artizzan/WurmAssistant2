using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Aldurcraft.Utility.Notifier
{
    [DataContract]
    public abstract class NotifierBase : NotifierAbstract, INotifier
    {
        public virtual bool HasEmptySound {
            get { return true; }
        }

        public abstract void Notify();

        public abstract INotifierConfig GetConfig();
    }
}
