using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Aldurcraft.Spellbook40.Extensions.System.Windows.Forms
{
    public static class TextBoxEx
    {
        /// <summary>
        /// Scrolls the view of multiline Textbox to bottom.
        /// </summary>
        /// <param name="tb"></param>
        public static void ScrollToBottomEx(this TextBox tb)
        {
            // autoscroll to bottom
            tb.SelectionStart = tb.Text.Length;
            tb.ScrollToCaret();
        }
    }
}
