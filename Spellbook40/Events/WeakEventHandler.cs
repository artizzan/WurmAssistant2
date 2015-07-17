using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Aldurcraft.Spellbook40.Events
{
    public delegate void UnregisterCallback<TEventArgs>(EventHandler<TEventArgs> eventHandler)
      where TEventArgs : EventArgs;

    public interface IWeakEventHandler<TEventArgs>
      where TEventArgs : EventArgs
    {
        EventHandler<TEventArgs> Handler { get; }
    }

    /// <summary>
    /// Weak event handler, does not prevent listener from being GC'd. Slower than regular event handlers.
    /// Useful for 3rd party code, plugins, scripts.
    /// </summary>
    /// <remarks>
    /// This class has been written by Dustin Campbell somewhere around ~2007
    /// http://diditwith.net/PermaLink,guid,aacdb8ae-7baa-4423-a953-c18c1c7940ab.aspx
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TEventArgs"></typeparam>
    public class WeakEventHandler<T, TEventArgs> : IWeakEventHandler<TEventArgs>
        where T : class
        where TEventArgs : EventArgs
    {
        private delegate void OpenEventHandler(T @this, object sender, TEventArgs e);

        private readonly WeakReference mTargetRef;
        private readonly OpenEventHandler mOpenHandler;
        private readonly EventHandler<TEventArgs> mHandler;
        private UnregisterCallback<TEventArgs> mUnregister;

        public WeakEventHandler(EventHandler<TEventArgs> eventHandler, UnregisterCallback<TEventArgs> unregister)
        {
            mTargetRef = new WeakReference(eventHandler.Target);
            mOpenHandler = (OpenEventHandler)Delegate.CreateDelegate(typeof(OpenEventHandler),
              null, eventHandler.Method);
            mHandler = Invoke;
            mUnregister = unregister;
        }

        public void Invoke(object sender, TEventArgs e)
        {
            T target = (T)mTargetRef.Target;

            if (target != null)
                mOpenHandler.Invoke(target, sender, e);
            else if (mUnregister != null)
            {
                mUnregister(mHandler);
                mUnregister = null;
            }
        }

        public EventHandler<TEventArgs> Handler
        {
            get { return mHandler; }
        }

        public static implicit operator EventHandler<TEventArgs>(WeakEventHandler<T, TEventArgs> weh)
        {
            return weh.mHandler;
        }
    }

    public static class EventHandlerUtils
    {
        public static EventHandler<TEventArgs> MakeWeak<TEventArgs>(
            this EventHandler<TEventArgs> eventHandler, UnregisterCallback<TEventArgs> unregister)
          where TEventArgs : EventArgs
        {
            if (eventHandler == null)
                throw new ArgumentNullException("eventHandler");
            if (eventHandler.Method.IsStatic || eventHandler.Target == null)
                throw new ArgumentException("Only instance methods are supported.", "eventHandler");

            Type wehType = typeof(WeakEventHandler<,>).MakeGenericType(eventHandler.Method.DeclaringType, typeof(TEventArgs));
            ConstructorInfo wehConstructor = wehType.GetConstructor(new Type[] { 
                typeof(EventHandler<TEventArgs>), 
                typeof(UnregisterCallback<TEventArgs>) });

            var weh = (IWeakEventHandler<TEventArgs>)wehConstructor.Invoke(
              new object[] { eventHandler, unregister });

            return weh.Handler;
        }
    }
}
