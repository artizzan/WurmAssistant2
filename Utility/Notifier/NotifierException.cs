using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aldurcraft.Utility.Notifier
{
    public class NotifierException : Exception
    {
        public NotifierException(string message) : base(message) {}
    }
}
