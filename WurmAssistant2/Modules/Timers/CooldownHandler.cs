using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aldurcraft.Utility.SoundEngine;
using Aldurcraft.Utility.PopupNotify;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Timers
{
    public class CooldownHandler
    {
        NotifyHandler Handler;
        DateTime _cooldownTo = DateTime.MinValue;
        public DateTime CooldownTo
        {
            get { return _cooldownTo; }
            set
            {
                if (value > DateTime.Now)
                {
                    shown = played = false;
                    _cooldownTo = value;
                }
            }
        }

        public bool SoundEnabled { get; set; }
        public bool PopupEnabled { get; set; }

        bool shown = true;
        bool played = true;

        public CooldownHandler(
            string soundName = null, string messageTitle = null, string messageContent = null, bool messagePersist = false)
        {
            Handler = new NotifyHandler();
            Handler.SoundName = (soundName ?? string.Empty);
            Handler.Title = (messageTitle ?? string.Empty);
            Handler.Message = (messageContent ?? string.Empty);
            if (messagePersist) Handler.PopupPersistent = true;
        }

        public void ResetShownAndPlayed()
        {
            shown = played = false;
        }

        public string SoundName
        {
            get { return Handler.SoundName; }
            set { Handler.SoundName = (value ?? ""); }
        }

        public string Title
        {
            get { return Handler.Title; }
            set { Handler.Title = (value ?? ""); }
        }

        public string Message
        {
            get { return Handler.Message; }
            set { Handler.Message = (value ?? ""); }
        }

        public bool PersistPopup
        {
            get { return Handler.PopupPersistent; }
            set { Handler.PopupPersistent = value; }
        }

        public int Duration
        {
            get { return Handler.Duration; }
            set { Handler.Duration = value; }
        }

        public void Update()
        {
            this.Update(false);
        }

        public void Update(bool engineSleeping)
        {
            if (DateTime.Now > CooldownTo)
            {
                if (SoundEnabled)
                {
                    if (!played)
                    {
                        Handler.Play();
                        played = true;
                    }
                }
                if (PopupEnabled)
                {
                    if (!shown)
                    {
                        Handler.Show();
                        shown = true;
                    }
                }
            }
            Handler.Update();
        }
    }

    class NotifyHandler
    {
        public string SoundName { get; set; }
        bool play;

        public string Title { get; set; }
        public string Message { get; set; }
        public bool PopupPersistent { get; set; }
        private int _Duration = 4000;
        /// <summary>
        /// Duration of the popup in milliseconds
        /// </summary>
        public int Duration { get { return _Duration; } set { _Duration = GeneralHelper.ConstrainValue<int>(value, 1000, int.MaxValue); } }
        bool show;

        public NotifyHandler()
            : this("", "", "", false)
        {
        }

        public NotifyHandler(string soundname, string messageTitle, string messageContent, bool messagePersist = false)
        {
            this.SoundName = soundname;
            this.Title = messageTitle;
            this.Message = messageContent;
            this.PopupPersistent = messagePersist;
        }

        public void Update()
        {
            if (play)
            {
                SoundBank.PlaySound(SoundName);
                Logger.LogDebug("played notify sound");
                play = false;
            }
            if (show)
            {
                if (PopupPersistent) Popup.Schedule(Title, Message, int.MaxValue);
                else Popup.Schedule(Title, Message, Duration);
                show = false;
            }
        }

        public void Play()
        {
            play = true;
        }

        public void Show()
        {
            show = true;
        }
    }
}
