using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility.MessageSystem;

namespace Aldurcraft.Utility.Notifier
{
    public interface INotifier
    {
        void Notify();
        INotifierConfig GetConfig();
        bool HasEmptySound { get; }
    }

    public interface ISoundNotifier
    {
        string SoundName { get; set; }
    }

    public interface IMessageNotifier
    {
        string Title { get; set; }
        string Content { get; set; }
    }

    public interface IPopupNotifier
    {
        string Title { get; set; }
        string Content { get; set; }
        TimeSpan Duration { get; set; }
        bool StayUntilClicked { get; set; }
    }

    public interface INotifierConfig
    {
        UserControl ControlHandle { get; }
        event EventHandler Removed;
    }
}
