using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aldurcraft.Utility.WinFormsManagers
{
    public class WidgetModeEventArgs : EventArgs
    {
        public bool WidgetMode { get; private set; }

        public WidgetModeEventArgs(bool widgetMode) : base()
        {
            WidgetMode = widgetMode;
        }
    }
}
