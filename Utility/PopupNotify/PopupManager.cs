using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Aldurcraft.Utility.PopupNotify
{
    class PopupManager
    {
        FormPopupContainer popupContainer;
        Thread popupThread;

        internal PopupManager()
        {
            BuildPopupThread();
        }

        void BuildPopupThread()
        {
            popupThread = new Thread(PopupThreadStart);
            popupThread.Priority = ThreadPriority.BelowNormal;
            popupThread.IsBackground = true;
            popupThread.Start();
        }

        void PopupThreadStart()
        {
            popupContainer = new FormPopupContainer();
            Application.Run(popupContainer);
        }

        internal void ScheduleCustomPopupNotify(string title, string content, int timeToShowMillis = 3000)
        {
            try
            {
                popupContainer.BeginInvoke(new Action<string, string, int>(popupContainer.ScheduleCustomPopupNotify), title, content, timeToShowMillis);
            }
            catch (Exception _e)
            {
                if (_e is NullReferenceException || _e is InvalidOperationException)
                {
                    Logger.LogError("! Invoke exception at ScheduleCustomPopupNotify:", this, _e);
                    try
                    {
                        if (_e is InvalidOperationException)
                        {
                            try { popupContainer.BeginInvoke(new Action(popupContainer.CloseThisContainer)); }
                            catch (Exception) { };
                        }
                        BuildPopupThread();
                        popupContainer.BeginInvoke(new Action<string, string, int>(popupContainer.ScheduleCustomPopupNotify), title, content, timeToShowMillis);
                    }
                    catch (Exception _e2)
                    {
                        Logger.LogError("! Fix failed", this, _e2);
                    }
                }
                else
                {
                    Logger.LogError("! Unknown Invoke exception at ScheduleCustomPopupNotify", this, _e);
                }
            }
        }

        internal void SetDefaultTitle(string title)
        {
            try
            {
                popupContainer.BeginInvoke(
                    new Action<string>(popupContainer.SetDefaultTitle), 
                    title);
            }
            catch (Exception _e)
            { Logger.LogError("! Invoke exception at ScheduleCustomPopupNotify:", this, _e); }
        }

        ~PopupManager()
        {
            try
            {
                popupContainer.BeginInvoke(new Action(popupContainer.CloseThisContainer));
            }
            catch 
            {
                System.Diagnostics.Debug.WriteLine("PopupManager finalizer exception");
            }
        }
    }
}
