using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Aldurcraft.Spellbook40.Events
{
    /// <summary>
    /// Creates a special EventHandler decorator, that protects delegate call chain from exceptions thrown in handling code.
    /// Intended for exposing sensitive events to 3rd party code, plugins, scripting, where failfast is not applicable.
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
    public class ScriptingEventHandler<T> where T : EventArgs
    {
        readonly Dictionary<EventHandler<T>, Stack<EventHandler<T>>> ehmap = new Dictionary<EventHandler<T>, Stack<EventHandler<T>>>();
        event EventHandler<T> DecoratedEvent;

        readonly Action<Exception, EventHandler<T>> exceptionAction;

        [Obsolete]
        static void UnsubscribeEventHandler(ScriptingEventHandler<T> csEventHandler, EventHandler<T> eh)
        {
            csEventHandler.Event -= eh;
        }

        [Obsolete]
        void UnsubscribeEventHandler(EventHandler<T> eh)
        {
            Event -= eh;
        }

        static readonly Action<Exception, EventHandler<T>> OverrideDefaultExceptionAction = null;

        /// <summary>
        /// Create new ScriptingEventHandler wrapper with default exceptionAction 
        /// (log exception, unsubscribe if nullref for weak events)
        /// </summary>
        public ScriptingEventHandler()
        {
            exceptionAction = new Action<Exception, EventHandler<T>>(
            (e, eh) =>
            {
                Debug.WriteLine(String.Format("ScriptingEventHandler caught exception,\r\nMESSAGE: {0}\r\nTRACE: {1}",
                    e.Message, e.StackTrace));
                if (e is NullReferenceException) this.Event -= eh; // UnsubscribeEventHandler(this, eh);
            });

            if (OverrideDefaultExceptionAction != null)
            {
                exceptionAction = OverrideDefaultExceptionAction;
            }
        }

        public ScriptingEventHandler(Action<Exception, EventHandler<T>> actionOnError) : this()
        {
            exceptionAction = actionOnError;
        }

        readonly object locker = new object();

        /// <summary>
        /// Subscribe to the wrapped event
        /// </summary>
        public event EventHandler<T> Event
        {
            add
            {
                lock (locker)
                {
                    var eh = new EventHandler<T>((o, e) =>
                    {
                        try { value(o, e); }
                        catch (Exception exception)
                        {
                            exceptionAction(exception, value);
                        }
                    });
                    DecoratedEvent += eh;
                    Stack<EventHandler<T>> handlerstack;
                    if (!ehmap.TryGetValue(value, out handlerstack))
                    {
                        handlerstack = new Stack<EventHandler<T>>();
                        ehmap[value] = handlerstack;
                    }
                    handlerstack.Push(eh);
                }
            }
            remove
            {
                lock (locker)
                {
                    Stack<EventHandler<T>> handlerstack;
                    if (ehmap.TryGetValue(value, out handlerstack))
                    {
                        //bug fixed by replacing counter with eh stack
                        DecoratedEvent -= handlerstack.Pop();
                        if (handlerstack.Count == 0) ehmap.Remove(value);
                    }
                    else Debug.WriteLine("Key Not Found");
                }
            }
        }

        public void Trigger(object sender, T eventArgs)
        {
            EventHandler<T> eh = DecoratedEvent;
            if (eh != null) eh(sender, eventArgs);
        }
    }
}
