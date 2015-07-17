using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Aldurcraft.Utility.MessageSystem
{
    public static class MessageRouter
    {
        public static event EventHandler<MessageRouterEventArgs> NewMessage;

        public static void SendMessage(Message message, object sender = null)
        {
            OnNewMessageSent(message);
        }

        static void OnNewMessageSent(Message message, object sender = null)
        {
            var eh = NewMessage;
            if (eh != null)
            {
                eh(sender, new MessageRouterEventArgs(message));
            }
        }
    }
}
