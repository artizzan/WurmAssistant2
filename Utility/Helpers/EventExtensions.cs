using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aldurcraft.Utility.Helpers
{
    public static class EventExtensions
    {
        public static void TriggerEventTsafe<TEventArgs>(object sender, TEventArgs eventArgs, EventHandler<TEventArgs> eventHandler) 
            where TEventArgs : EventArgs
        {
            if (eventHandler != null)
            {
                eventHandler(sender, eventArgs);
            }
        }

        public static void TriggerEventTsafe(object sender, EventArgs eventArgs, EventHandler eventHandler)
        {
            if (eventHandler != null)
            {
                eventHandler(sender, eventArgs);
            }
        }
    }
}
