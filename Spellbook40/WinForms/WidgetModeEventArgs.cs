using System;

namespace Aldurcraft.Spellbook40.WinForms
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
