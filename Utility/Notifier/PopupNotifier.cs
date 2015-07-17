using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Aldurcraft.Utility.MessageSystem;
using Aldurcraft.Utility.PopupNotify;
using Aldurcraft.Utility.SoundEngine;
using Aldurcraft.Utility;

namespace Aldurcraft.Utility.Notifier
{
    [DataContract]
    public class PopupNotifier : NotifierBase, IPopupNotifier
    {
        [DataMember]
        private PopupMessage _popupMessage;
        [DataMember] 
        private bool _stayUntilClicked;

        protected PopupMessage PopupMessage
        {
            get { return _popupMessage; }
            set { _popupMessage = value; }
        }

        public bool StayUntilClicked
        {
            get { return _stayUntilClicked; }
            set { _stayUntilClicked = value; }
        }

        public override void Notify()
        {
            PopupMessage.Send(StayUntilClicked);
        }

        public PopupNotifier(PopupMessage message)
        {
            if (message == null) throw new NotifierException("message can't be null");
            PopupMessage = message;
        }

        public string Content { get { return _popupMessage.Content; } set { _popupMessage.Content = value; } }
        public string Title { get { return _popupMessage.Title; } set { _popupMessage.Title = value; } }

        public TimeSpan Duration
        {
            get { return TimeSpan.FromMilliseconds(_popupMessage.Duration); }
            set { _popupMessage.Duration = (int)value.TotalMilliseconds; }
        }

        public override INotifierConfig GetConfig()
        {
            return new PopupConfig(this);
        }
    }
}
