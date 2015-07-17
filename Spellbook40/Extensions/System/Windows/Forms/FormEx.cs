using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Aldurcraft.Spellbook40.Extensions.System.Windows.Forms
{
    public static class FormEx
    {
        /// <summary>
        /// Shows this window centered at parent, additionally fits the window into work area if it's outside bounds
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        public static void ShowCenteredEx(this Form child, Form parent)
        {
            SetCenteredOnParentOnLoadWorkAreaBoundEx(child, parent);
            child.Show();
        }

        public static DialogResult ShowDialogCenteredEx(this Form child, Form parent)
        {
            SetCenteredOnParentOnLoadWorkAreaBoundEx(child, parent);
            return child.ShowDialog();
        }

        /// <summary>
        /// Restores shape of the form from saved rectangle, additionally fits the window into work area if it's outside bounds or too large
        /// </summary>
        /// <param name="form"></param>
        /// <param name="savedShape"></param>
        public static void RestoreShapeEx(this Form form, Rectangle savedShape)
        {
            SetFormShapeWorkAreaBoundEx(form, savedShape);
        }

        /// <summary>
        /// Returns shape of the form, which is Form.Location and Form.Size when visible and Form.RestoreBounds when hidden / minimized
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public static Rectangle GetShapeEx(this Form form)
        {
            return GetFormRealBoundsEx(form);
        }

        public static void ShowThisDarnWindowDammitEx(this Form form)
        {
            if (form.Visible)
            {
                if (form.WindowState == FormWindowState.Minimized) form.WindowState = FormWindowState.Normal;
                form.BringToFront();
            }
            else
            {
                form.Show();
                if (form.WindowState == FormWindowState.Minimized) form.WindowState = FormWindowState.Normal;
            }
            FitWindowIntoWorkAreaEx(form);
        }

        public static Point GetCenteredChildPositionRelativeToParentWorkAreaBoundEx(this Form parent, Form child)
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

        public static void FitWindowIntoWorkAreaEx(this Form form)
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

        public static void SetCenteredOnParentOnLoadWorkAreaBoundEx(this Form parent, Form child)
        {
            child.Load += (sender, args) => child.Location = GetCenteredChildPositionRelativeToParentWorkAreaBoundEx(child, parent);
        }

        public static void SetFormShapeWorkAreaBoundEx(this Form form, Rectangle shape)
        {
            form.Location = new Point(shape.X, shape.Y);
            form.Size = new Size(shape.Width, shape.Height);
            FitWindowIntoWorkAreaEx(form);
        }

        public static Rectangle GetFormRealBoundsEx(this Form form)
        {
            if (form.Visible) return new Rectangle(form.Location.X, form.Location.Y, form.Size.Width, form.Size.Height);
            else return form.RestoreBounds;
        }
    }
}
