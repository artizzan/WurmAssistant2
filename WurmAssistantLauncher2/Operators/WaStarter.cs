using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aldurcraft.WurmAssistantMutexes;
using WurmAssistantLauncher2.Models;
using WurmAssistantLauncher2.Utility;

namespace WurmAssistantLauncher2.Operators
{
    abstract class WaStarter
    {
        // no need to run under WA mutex, WA will handle this by itself
        protected string BasePath;
        protected string BuildType;
        /// <summary>
        /// Launches latest version of WA
        /// </summary>
        public void Launch()
        {
            if (string.IsNullOrWhiteSpace(BasePath)) throw new LauncherException("no path specified");

            var latest = WurmAssistantSpellbook.GetLatestVersionDirAtLocation(BasePath);
            var psi = new ProcessStartInfo(Path.Combine(latest.FullName, "WurmAssistant.exe"))
            {
                Arguments = "-b " + BuildType
            };
            Process.Start(psi);
        }
    }

    class BetaWaStarter : WaStarter
    {
        public BetaWaStarter()
        {
            BasePath = AppContext.BetaInstallDir;
            BuildType = AppContext.BetaKey;
        }
    }

    class StableWaStarter : WaStarter
    {
        public StableWaStarter()
        {
            BasePath = AppContext.StableInstallDir;
            BuildType = AppContext.StableKey;
        }
    }
}
