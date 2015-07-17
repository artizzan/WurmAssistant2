using System;
using System.Reflection;
using System.Threading.Tasks;
using Aldurcraft.Spellbook40.Extensions.System.Reflection;
using Aldurcraft.Spellbook40.Transient;
using Aldurcraft.Spellbook40.WebApi;
using Aldurcraft.WurmAssistantLauncher2.Models;

namespace Aldurcraft.WurmAssistantLauncher2.Managers
{
    class UpdateLauncherManager
    {
        public Version CurrentLauncherVersion { get; private set; }

        public UpdateLauncherManager()
        {
            CurrentLauncherVersion = AssemblyEx.EntryAssemblyVersion;
        }

        public async Task<bool> IsUpdateAvailable()
        {
            var latest = await TransientHelper.CompensateAsync(GetLatestVersionInfo, "Failed to check launcher version");
            return CurrentLauncherVersion < latest;
        }

        async Task<Version> GetLatestVersionInfo()
        {
            // ask web service if available
            return await WebApiEx.GetObjectFromWebApiAsync<Version>(
                AppContext.WebApiBasePath,
                string.Format("LatestLauncherVersion/{0}", AppContext.ProjectName),
                TimeSpan.FromSeconds(15));
        }
    }
}
