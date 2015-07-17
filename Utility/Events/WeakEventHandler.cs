using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Aldurcraft.Utility.Events
{
    public delegate void UnregisterCallback<E>(EventHandler<E> eventHandler)
      where E : EventArgs;

    public interface IWeakEventHandler<E>
      where E : EventArgs
    {
        EventHandler<E> Handler { get; }
    }

    /// <summary>
    /// Weak event handler, does not prevent listener from being GC'd. Slower than regular event handlers.
    /// Useful for subscriptions coming from 3rd party code, plugins, scripts.
    /// </summary>
    /// <remarks>
    /// This class has been written by Dustin Campbell somewhere around ~2007
    /// http://diditwith.net/PermaLink,guid,aacdb8ae-7baa-4423-a953-c18c1c7940ab.aspx
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    public class WeakEventHandler<T, E> : IWeakEventHandler<E>
        where T : class
        where E : EventArgs
    {
        private delegate void OpenEventHandler(T @this, object sender, E e);

        private WeakReference m_TargetRef;
        private OpenEventHandler m_OpenHandler;
        private EventHandler<E> m_Handler;
        private UnregisterCallback<E> m_Unregister;

        public WeakEventHandler(EventHandler<E> eventHandler, UnregisterCallback<E> unregister)
        {
            m_TargetRef = new WeakReference(eventHandler.Target);
            m_OpenHandler = (OpenEventHandler)Delegate.CreateDelegate(typeof(OpenEventHandler),
              null, eventHandler.Method);
            m_Handler = Invoke;
            m_Unregister = unregister;
        }

        public void Invoke(object sender, E e)
        {
            T target = (T)m_TargetRef.Target;

            if (target != null)
                m_OpenHandler.Invoke(target, sender, e);
            else if (m_Unregister != null)
            {
                m_Unregister(m_Handler);
                m_Unregister = null;
            }
        }

        public EventHandler<E> Handler
        {
            get { return m_Handler; }
        }

        public static implicit operator EventHandler<E>(WeakEventHandler<T, E> weh)
        {
            return weh.m_Handler;
        }
    }

    public static class EventHandlerUtils
    {
        public static EventHandler<E> MakeWeak<E>(this EventHandler<E> eventHandler, UnregisterCallback<E> unregister)
          where E : EventArgs
        {
            if (eventHandler == null)
                throw new ArgumentNullException("eventHandler");
            if (eventHandler.Method.IsStatic || eventHandler.Target == null)
                throw new ArgumentException("Only instance methods are supported.", "eventHandler");

            Type wehType = typeof(WeakEventHandler<,>).MakeGenericType(eventHandler.Method.DeclaringType, typeof(E));
            ConstructorInfo wehConstructor = wehType.GetConstructor(new Type[] { 
                typeof(EventHandler<E>), 
                typeof(UnregisterCallback<E>) });

            IWeakEventHandler<E> weh = (IWeakEventHandler<E>)wehConstructor.Invoke(
              new object[] { eventHandler, unregister });

            return weh.Handler;
        }
    }
}
