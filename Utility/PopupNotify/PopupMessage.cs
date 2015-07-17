using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Aldurcraft.Utility.PopupNotify
{
    [DataContract]
    public class PopupMessage
    {
        [DataMember]
        private string _title;
        [DataMember]
        private string _content;
        [DataMember]
        private int _duration;

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

        public int Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        public void Send()
        {
            Popup.Schedule(Title, Content, Duration);
        }

        public void Send(bool stayUntilClicked)
        {
            if (stayUntilClicked) Popup.Schedule(Title, Content, int.MaxValue);
            else Send();
        }

        public void Send(string[] args)
        {
            var formattedContent = string.Format(Content, args);
            Popup.Schedule(Title, formattedContent, Duration);
        }
    }
}
