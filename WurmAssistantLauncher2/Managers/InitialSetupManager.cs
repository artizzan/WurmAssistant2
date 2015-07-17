using System.IO;
using System.Linq;
using Aldurcraft.Spellbook40.Extensions.System.IO;
using Aldurcraft.WurmAssistantLauncher2.Models;
using Aldurcraft.WurmAssistantMutexes;

namespace Aldurcraft.WurmAssistantLauncher2.Managers
{
    public class InitialSetupManager
    {
        // run under WA mutex
        public void Execute()
        {
            var dirSet = new[]
            {
                AppContext.BetaInstallDir,
                AppContext.BetaDataDir,
                AppContext.StableInstallDir,
                AppContext.StableDataDir,
                AppContext.LauncherSettingsDir,
                AppContext.TempDir,
                AppContext.WaBackupBetaDir,
                AppContext.WaBackupStableDir
            };
            bool dirsExist = CheckIfDirsExist(dirSet);

            bool portingOldDataDirRequired = false;
            var stableDir = new DirectoryInfo(AppContext.StableDataDir);
            if (!stableDir.Exists
                || (!stableDir.GetFiles().Any() && !stableDir.GetDirectories().Any()))
            {
                if (Directory.Exists(AppContext.OldDataDir))
                {
                    portingOldDataDirRequired = true;
                }
            }

            if (!dirsExist || portingOldDataDirRequired)
            {
                using (new WurmAssistantGateway(AppContext.WaGatewayErrorMessage))
                {
                    if (!dirsExist) CreateDirsIfNotExists(dirSet);

                    if (portingOldDataDirRequired) CopyExistingWurmAssistantSettingsIfExist();
                }
            }
        }

        private void CopyExistingWurmAssistantSettingsIfExist()
        {
            DirectoryEx.DirectoryCopyAdv(AppContext.OldDataDir, AppContext.StableDataDir);
        }

        void CreateDirIfNotExists(string dirName)
        {
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
        }

        void CreateDirsIfNotExists(params string[] dirNames)
        {
            foreach (var dirName in dirNames)
            {
                CreateDirIfNotExists(dirName);
            }
        }

        bool CheckIfDirsExist(params string[] dirNames)
        {
            return dirNames.All(Directory.Exists);
        }
    }
}
