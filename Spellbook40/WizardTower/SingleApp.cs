using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Aldurcraft.Spellbook40.WizardTower
{
    /// <summary>
    /// Note: this fails if app is minimzed to tray or if the very first 
    /// window handle is not the main window of target app
    /// In short: it sucks.
    /// </summary>
    [Obsolete("Use PipeCom instead")]
    public static class SingleApp
    {
        [DllImport("user32.dll")]
        private static extern
            bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern
            bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern
            bool IsIconic(IntPtr hWnd);

        const int SwRestore = 9;

        public static Process GetOtherProcessIfRunning()
        {
            var currentProcess = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(currentProcess.ProcessName);

            if (processes.Any())
            {
                return processes.FirstOrDefault(x => x.Id != currentProcess.Id);
            }

            return null;
        }

        public static void ShowMainWindow(Process process)
        {
            // get the window handle
            IntPtr hWnd = process.MainWindowHandle;

            // if iconic, we need to restore the window
            if (IsIconic(hWnd))
            {
                ShowWindowAsync(hWnd, SwRestore);
            }

            // bring it to the foreground
            SetForegroundWindow(hWnd);
        }
    }
}
