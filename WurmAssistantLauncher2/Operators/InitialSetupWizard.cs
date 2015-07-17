using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aldurcraft.WurmAssistantMutexes;
using WurmAssistantLauncher2.Models;
using WurmAssistantLauncher2.Utility;

namespace WurmAssistantLauncher2.Operators
{
    class InitialSetupWizard
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

            if (!dirsExist || !portingOldDataDirRequired)
            {
                using (var mutex = new WurmAssistantMutex())
                {
                    mutex.Enter(1000, "Wurm Assistant must be closed before launcher can finish initial setup");

                    if (!dirsExist) CreateDirsIfNotExists(dirSet);

                    if (!portingOldDataDirRequired) CopyExistingWurmAssistantSettingsIfExist();
                }
            }
        }

        private void CopyExistingWurmAssistantSettingsIfExist()
        {
            IoHelper.AdvDirectoryCopy(AppContext.OldDataDir, AppContext.StableDataDir);
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
            return dirNames.Any(x => !Directory.Exists(x));
        }
    }
}
