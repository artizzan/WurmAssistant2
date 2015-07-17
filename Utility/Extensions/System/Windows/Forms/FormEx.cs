using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility;

namespace System.Windows.Forms
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
            Aldurcraft.Utility.FormHelper.SetCenteredOnParentOnLoadWorkAreaBound(child, parent);
            child.Show();
        }

        public static DialogResult ShowDialogCenteredEx(this Form child, Form parent)
        {
            Aldurcraft.Utility.FormHelper.SetCenteredOnParentOnLoadWorkAreaBound(child, parent);
            return child.ShowDialog();
        }

        /// <summary>
        /// Restores shape of the form from saved rectangle, additionally fits the window into work area if it's outside bounds or too large
        /// </summary>
        /// <param name="form"></param>
        /// <param name="savedShape"></param>
        public static void RestoreShapeEx(this Form form, Rectangle savedShape)
        {
            Aldurcraft.Utility.FormHelper.SetFormShapeWorkAreaBound(form, savedShape);
        }

        /// <summary>
        /// Returns shape of the form, which is Form.Location and Form.Size when visible and Form.RestoreBounds when hidden / minimized
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public static Rectangle GetShapeEx(this Form form)
        {
            return Aldurcraft.Utility.FormHelper.GetFormRealBounds(form);
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
            FormHelper.FitWindowIntoWorkArea(form);
        }
    }
}
