using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aldurcraft.WurmAssistantMutexes;

namespace WurmAssistantLauncher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                using (var mutex = new WurmAssistantGateway())
                {

                    mutex.Enter(1000);
                    Application.Run(new FormLauncher());
                }
            }
            catch (GatewayClosedException)
            {
                MessageBox.Show("Wurm Assistant is already running! Please close before starting this launcher.",
                    "Wurm Assistant", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
