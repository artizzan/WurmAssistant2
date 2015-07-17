using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Aldurcraft.Utility.MessageSystem
{
    [DataContract]
    public class Message : IMessage, IComparable, IComparable<Message>
    {
        [DataMember]
        private string _title;
        [DataMember]
        private string _content;
        [DataMember]
        private DateTime _timeStamp;
        [DataMember]
        private string _source;
        [DataMember]
        private string _player;

        public Message() : this(DateTime.Now) {}

        internal Message(DateTime stamp)
        {
            Title = string.Empty;
            Content = string.Empty;
            TimeStamp = stamp;
            _source = string.Empty;
            _player = string.Empty;
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }

        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }

        public string Source
        {
            get { return _source; }
            set { _source = value; }
        }

        public string Player
        {
            get { return _player; }
            set { _player = value; }
        }


        public int CompareTo(Message other)
        {
            return TimeStamp.CompareTo(other.TimeStamp);
        }

        public int CompareTo(object other)
        {
            var message = other as Message;
            if (message != null)
            {
                return CompareTo(message);
            }
            else throw new InvalidOperationException("other is not Message");
        }

        public Message Clone()
        {
            return new Message()
                   {
                       Title = this.Title,
                       Content = this.Content,
                       TimeStamp = DateTime.Now,
                       Player = this.Player,
                       Source = this.Source
                   };
        }

        public Message SendClone()
        {
            var copy = this.Clone();
            MessageRouter.SendMessage(copy);
            return copy;
        }
    }
}
