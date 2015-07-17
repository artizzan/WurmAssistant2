using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Aldurcraft.Utility.Helpers;

namespace Aldurcraft.Utility
{
    /// <summary>
    /// Utilities for working with WinForms
    /// </summary>
    public static class FormHelper
    {
        /// <summary>
        /// Returns a Point intended for Location of child form, so that it appears centered in regard to parent.
        /// Known issue: form can appear with title bar or other edges outside visible desktop area.
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static Point GetCenteredChildPositionRelativeToParentWorkAreaBound(Form child, Form parent)
        {
            Size newFormSize = child.Size;
            Size parentFormSize = parent.Size;
            Point parentFormLocation = parent.Location;
            Point parentCenter = new Point(parentFormSize.Width / 2, parentFormSize.Height / 2);
            Point parentAdjLocation = new Point(parentFormLocation.X + parentCenter.X, parentFormLocation.Y + parentCenter.Y);

            Point newFormOffset = new Point(newFormSize.Width / 2, newFormSize.Height / 2);
            var newLoc = new Point(parentAdjLocation.X - newFormOffset.X, parentAdjLocation.Y - newFormOffset.Y);

            var workingArea = Screen.FromControl(parent).WorkingArea;

            if (newLoc.X < workingArea.X) newLoc.X = workingArea.X;
            else if (newLoc.X + child.Width > workingArea.Right) newLoc.X = workingArea.Right - child.Width;

            if (newLoc.Y < workingArea.Y) newLoc.Y = workingArea.Y;
            else if (newLoc.Y + child.Height > workingArea.Bottom) newLoc.Y = workingArea.Bottom - child.Height;

            return newLoc;
        }

        public static void FitWindowIntoWorkArea(Form form)
        {
            var workingArea = Screen.FromControl(form).WorkingArea;
            
            // make sure form is not bigger than working area
            if (workingArea.Width < form.Size.Width) form.Size = new Size(workingArea.Width, form.Size.Height);
            if (workingArea.Height < form.Size.Height) form.Size = new Size(form.Size.Width, workingArea.Height);

            Point newLoc = form.Location;

            // make sure it is fully visible in X axis
            if (form.Location.X < workingArea.X) newLoc.X = workingArea.X;
            else if (form.Location.X + form.Size.Width > workingArea.Right) newLoc.X = workingArea.Right - form.Size.Width;

            // make sure it is fully visible in Y axis
            if (form.Location.Y < workingArea.Y) newLoc.Y = workingArea.Y;
            else if (form.Location.Y + form.Size.Height > workingArea.Bottom) newLoc.Y = workingArea.Bottom - form.Size.Height;

            form.Location = newLoc;
        }

        public static void SetCenteredOnParentOnLoadWorkAreaBound(Form child, Form parent)
        {
            child.Load += (sender, args) => child.Location = GetCenteredChildPositionRelativeToParentWorkAreaBound(child, parent);
        }

        public static void SetFormShapeWorkAreaBound(Form form, Rectangle savedShape)
        {
            form.Location = new Point(savedShape.X, savedShape.Y);
            form.Size = new Size(savedShape.Width, savedShape.Height);
            FitWindowIntoWorkArea(form);
        }

        public static Rectangle GetFormRealBounds(Form form)
        {
            if (form.Visible) return new Rectangle(form.Location.X, form.Location.Y, form.Size.Width, form.Size.Height);
            else return form.RestoreBounds;
        }

        /// <summary>
        /// Scrolls the view of multiline Textbox to bottom.
        /// </summary>
        /// <param name="tb"></param>
        public static void TextBoxScrollToBottom(TextBox tb)
        {
            // autoscroll to bottom
            tb.SelectionStart = tb.Text.Length;
            tb.ScrollToCaret();
        }
    }
}
