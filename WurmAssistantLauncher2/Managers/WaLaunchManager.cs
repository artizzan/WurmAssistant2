using System.Diagnostics;
using System.IO;
using Aldurcraft.WurmAssistantLauncher2.Models;
using Aldurcraft.WurmAssistantLauncher2.Utility;

namespace Aldurcraft.WurmAssistantLauncher2.Managers
{
    abstract class WaLaunchManager
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
            var psi = new ProcessStartInfo(Path.Combine(latest.FullName, "WurmAssistant2.exe"))
            {
                Arguments = "-b " + BuildType
            };
            Process.Start(psi);
        }
    }

    class BetaWaLaunchManager : WaLaunchManager
    {
        public BetaWaLaunchManager()
        {
            BasePath = AppContext.BetaInstallDir;
            BuildType = AppContext.BetaKey;
        }
    }

    class StableWaLaunchManager : WaLaunchManager
    {
        public StableWaLaunchManager()
        {
            BasePath = AppContext.StableInstallDir;
            BuildType = AppContext.StableKey;
        }
    }
}
