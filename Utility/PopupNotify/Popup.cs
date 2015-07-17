using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aldurcraft.Utility.PopupNotify
{
    /// <summary>
    /// A simple system for showing popup windows in bottom-right-most side of main screen. 
    /// Check remarks for required library.
    /// </summary>
    /// <remarks>
    /// Required library: http://www.codeproject.com/Articles/277584/Notification-Window
    /// This wrapper runs NotificationWindow in another thread to minimize the likelyhood,
    /// that popup will steal user focus, however it still sometimes happens and the cause is unknown.
    /// </remarks>
    public static class Popup
    {
        static PopupManager Manager1;

        public static void Initialize()
        {
            Manager1 = new PopupManager();
        }

        /// <summary>
        /// Add message to Popup queue
        /// </summary>
        /// <param name="title">title of the message</param>
        /// <param name="content">content of the message</param>
        /// <param name="timeToShow">how long should this popup be visible</param>
        public static void Schedule(string title, string content, int timeToShow = 3000)
        {
            Manager1.ScheduleCustomPopupNotify(title, content, timeToShow);
        }

        /// <summary>
        /// Add message to Popup queue with default title
        /// </summary>
        /// <param name="content">content of the message</param>
        /// <param name="timeToShow">how long should this popup be visible</param>
        public static void Schedule(string content, int timeToShow = 3000)
        {
            Schedule(null, content, timeToShow);
        }

        /// <summary>
        /// Set default title for messages
        /// </summary>
        /// <param name="newTitle"></param>
        public static void SetDefaultTitle(string newTitle)
        {
            Manager1.SetDefaultTitle(newTitle ?? "NULL");
        }
    }
}
