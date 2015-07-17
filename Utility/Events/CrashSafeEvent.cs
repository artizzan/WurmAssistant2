using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Aldurcraft.Utility.Events
{
    /// <summary>
    /// Creates a special event handler wrapper, that protects delegate call chain from exceptions thrown in handling code.
    /// Intended for exposing sensitive events to 3rd party code, plugins, scripting. Designed for thread safety (untested).
    /// </summary>
    /// <remarks>
    /// What this class does, is essentially wrapping regular event handler into another event handler,
    /// where the method invocation is protected by a try-catch block. Upon exception being caught,
    /// error message is logged and optionally custom Action can be triggered.
    /// Such action takes 2 arguments: the exception object and the actual EventHandler that crashed.
    /// This can be used to forcefully unsubscribe such handler or track the source of exception
    /// and kill the plugin / script that causes issues.
    /// </remarks>
    /// <typeparam name="T">where T : EventArgs</typeparam>
    public class CrashSafeEvent<T> where T : EventArgs
    {
        Dictionary<EventHandler<T>, Stack<EventHandler<T>>> EHMAP = new Dictionary<EventHandler<T>, Stack<EventHandler<T>>>();
        event EventHandler<T> _Event;

        Action<Exception, EventHandler<T>> exceptionAction;

        [Obsolete]
        static void UnsubscribeEventHandler(CrashSafeEvent<T> csEvent, EventHandler<T> eh)
        {
            csEvent.Event -= eh;
        }

        [Obsolete]
        void UnsubscribeEventHandler(EventHandler<T> eh)
        {
            Event -= eh;
        }

        static Action<Exception, EventHandler<T>> overrideDefaultExceptionAction = null;

        /// <summary>
        /// Create new CrashSafeEvent wrapper with default exceptionAction 
        /// (log exception, unsubscribe if nullref for weak events)
        /// </summary>
        public CrashSafeEvent()
        {
            exceptionAction = new Action<Exception, EventHandler<T>>(
            (e, eh) =>
            {
                Debug.WriteLine(String.Format("CrashSafeEvent caught exception,\r\nMESSAGE: {0}\r\nTRACE: {1}",
                    e.Message, e.StackTrace));
                if (e is NullReferenceException) this.Event -= eh; // UnsubscribeEventHandler(this, eh);
            });

            if (overrideDefaultExceptionAction != null)
            {
                exceptionAction = overrideDefaultExceptionAction;
            }
        }

        public CrashSafeEvent(Action<Exception, EventHandler<T>> actionOnError) : this()
        {
            exceptionAction = actionOnError;
        }

        object _locker = new object();

        /// <summary>
        /// Subscribe to the wrapped event
        /// </summary>
        public event EventHandler<T> Event
        {
            add
            {
                lock (_locker)
                {
                    EventHandler<T> eh = new EventHandler<T>((o, e) =>
                    {
                        try { value(o, e); }
                        catch (Exception exception)
                        {
                            exceptionAction(exception, value);
                        }
                    });
                    _Event += eh;
                    Stack<EventHandler<T>> handlerstack;
                    if (!EHMAP.TryGetValue(value, out handlerstack))
                    {
                        handlerstack = new Stack<EventHandler<T>>();
                        EHMAP[value] = handlerstack;
                    }
                    handlerstack.Push(eh);
                }
            }
            remove
            {
                lock (_locker)
                {
                    Stack<EventHandler<T>> handlerstack;
                    if (EHMAP.TryGetValue(value, out handlerstack))
                    {
                        //bug fixed by replacing counter with eh stack
                        _Event -= handlerstack.Pop();
                        if (handlerstack.Count == 0) EHMAP.Remove(value);
                    }
                    else Debug.WriteLine("Key Not Found");
                }
            }
        }

        /// <summary>
        /// Optimized faster threadsafe trigger, uses only memory barrier, not guaranteed to be t-safe outside NET 2.0 to 4.5
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public void TriggerNET4_TSafe(object sender, T eventArgs)
        {
            EventHandler<T> temp = _Event;
            System.Threading.Thread.MemoryBarrier();
            if (temp != null) temp(sender, eventArgs);
        }

        /// <summary>
        /// Triggers the event in thread-safe way (does NOT protect from another event triggering before last one finishes)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public void Trigger(object sender, T eventArgs)
        {
            EventHandler<T> temp;
            lock (_locker)
            {
                temp = _Event;
            }
            if (temp != null) temp(sender, eventArgs);
        }
    }
}
