using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aldurcraft.Utility;
using Aldurcraft.WurmAssistantMutexes;

namespace Aldurcraft.WurmOnline.WurmAssistant2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                using (var mutex = new WurmAssistantGateway())
                {

                    mutex.Enter(1000);
                    Application.Run(new WurmAssistant());
                }
                
            }
            catch (GatewayClosedException)
            {
                MessageBox.Show("Wurm Assistant is already running!", 
                    "Wurm Assistant", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        // this is pretty much useless!
        static void HandleUncaughtException(Exception e)
        {
            Logger.LogCritical("UncaughtException", "GLOBAL", e);
            FormUncaughtException exceptionGUI = new FormUncaughtException(e,
                new string[] 
                { 
                    "Running OS: " + OperatingSystemInfo.RunningOS_Raw, 
                    "Assistant version: " + Application.ProductVersion,
                });
            exceptionGUI.ShowDialog();
            Application.Exit();
        }
    }
}
