using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Aldurcraft.Utility.Helpers;
using Aldurcraft.Utility.NativeHooks;

namespace Aldurcraft.Utility.WinFormsManagers
{
    public class WidgetModeManager
    {
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        private bool _widgetModeEnabled = false;
        private bool _widgetMode = false;
        private readonly Form _form;
        private readonly Color _transparencyBackColor = Color.Fuchsia;

        private readonly Color _defBackColor;
        private readonly Color _defTransparencyKey;
        private readonly FormBorderStyle _defBorderStyle;

        public event EventHandler<WidgetModeEventArgs> WidgetModeChanging;

        public WidgetModeManager(Form form)
        {
            this._form = form;
            _defBackColor = form.BackColor;
            _defTransparencyKey = form.TransparencyKey;
            _defBorderStyle = form.FormBorderStyle;

            //set middle-mouse form-wide for toggling modes
            SetMouseEvents(form);
        }

        public bool WidgetMode { get { return _widgetMode; } }

        void SetMouseEvents(Control control)
        {
            control.MouseDown -= CtrlMouseClick;
            control.MouseDown += CtrlMouseClick;
            var ctrls = control.Controls;
            foreach (Control ctrl in ctrls)
            {
                SetMouseEvents(ctrl);
            }
        }

        public void ResetMouseEvents()
        {
            SetMouseEvents(_form);
        }

        void CtrlMouseClick(object sender, MouseEventArgs e)
        {
            if (_widgetModeEnabled)
            {
                // alternative widget mode toggle
                if (e.Button == MouseButtons.Middle ||
                    (e.Button == MouseButtons.Right &&
                    (Control.ModifierKeys & Keys.Shift) != 0 &&
                    (Control.ModifierKeys & Keys.Alt) != 0 &&
                    (Control.ModifierKeys & Keys.Control) != 0))
                {
                    Toggle();
                }

                // window dragging while in widget mode
                if (_widgetMode)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        ReleaseCapture();
                        SendMessage(_form.Handle, Win32Hooks.WM_NCLBUTTONDOWN, Win32Hooks.HT_CAPTION, 0);
                    }
                }
            }
            else
            {
                Toggle(true);
            }
        }

        public void Set(bool enabled)
        {
            _widgetModeEnabled = enabled;
        }

        private void Toggle(bool forceOutOfWidgetMode = false)
        {
            if (forceOutOfWidgetMode)
            {
                _widgetMode = false;
            }
            else
            {
                _widgetMode = !_widgetMode;
            }

            OnWidgetModeChanging();

            if (_widgetMode)
            {
                _form.FormBorderStyle = FormBorderStyle.None;
                _form.BackColor = _form.TransparencyKey = _transparencyBackColor;
                _form.TopMost = true;
            }
            else
            {
                _form.BackColor = _form.TransparencyKey = _defTransparencyKey;
                _form.FormBorderStyle = _defBorderStyle;
                _form.TopMost = false;
            }
        }

        private void OnWidgetModeChanging()
        {
            EventExtensions.TriggerEventTsafe(this, new WidgetModeEventArgs(_widgetMode), WidgetModeChanging);
        }
    }
}
