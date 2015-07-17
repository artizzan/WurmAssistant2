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
    public class MessageNotifier : NotifierBase, IMessageNotifier
    {
        [DataMember]
        private Message _message;

        protected Message Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public MessageNotifier(Message message)
        {
            if (message == null) throw new NotifierException("message can't be null");
            Message = message;
        }

        public override void Notify()
        {
            Message.SendClone();
        }

        public string Content { get { return _message.Content; } set { _message.Content = value; } }
        public string Title { get { return _message.Title; } set { _message.Title = value; } }

        public override INotifierConfig GetConfig()
        {
            return new MessageConfig(this);
        }
    }
}
