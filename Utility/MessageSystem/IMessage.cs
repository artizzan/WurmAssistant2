using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aldurcraft.Utility.MessageSystem
{
    public interface IMessage
    {
        string Title { get; set; }

        string Content { get; set; }

        string Source { get; set; }

        string Player { get; set; }

        DateTime TimeStamp { get; set; }
    }
}
