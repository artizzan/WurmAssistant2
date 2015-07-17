using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aldurcraft.Utility.MessageSystem
{
    public class MessageRouterEventArgs : EventArgs
    {
        public readonly Message Message;

        public MessageRouterEventArgs(Message message)
        {
            Message = message;
        }
    }
}
