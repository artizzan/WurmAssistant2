using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Aldurcraft.Spellbook40.Extensions.System;
using Aldurcraft.Spellbook40.Extensions.System.IO;
using Aldurcraft.Spellbook40.Transient;
using Aldurcraft.WurmAssistantLauncher2.Models;
using Aldurcraft.WurmAssistantLauncher2.ViewModels;
using Aldurcraft.WurmAssistantMutexes;
using SevenZip;

namespace Aldurcraft.WurmAssistantLauncher2.Managers
{
    public abstract class BackupsManager
    {
        private readonly WaBackupsViewModel waBackupsViewModel;
        public string DataDirPath { get; protected set; }
        protected string BackupDirPath;
        protected string BuildType;

        protected BackupsManager(WaBackupsViewModel waBackupsViewModel)
        {
            this.waBackupsViewModel = waBackupsViewModel;
        }

        public WaBackup CreateBackup(string backupName = null)
        {
            return WaBackup.Create(backupName, BuildType, DataDirPath, BackupDirPath, this, waBackupsViewModel);
        }

        public void ImportSettings(BuildType buildType)
        {
            using (new WurmAssistantGateway(AppContext.WaGatewayErrorMessage))
            {
                using (var tempDirMan = new TempDirManager())
                {
                    var tempDir = tempDirMan.GetHandle();
                    var dataDir = new DirectoryInfo(DataDirPath);

                    var tempBackupDir = new DirectoryInfo(Path.Combine(tempDir.FullName, dataDir.Name));
                    DirectoryEx.DirectoryCopyRecursive(dataDir.FullName, tempBackupDir.FullName);

                    try
                    {
                        dataDir.Delete(true);
                        var otherBuildDirPath = AppContext.GetDataDir(buildType);
                        DirectoryEx.DirectoryCopyRecursive(otherBuildDirPath, dataDir.FullName);
                    }
                    catch (Exception)
                    {
                        TransientHelper.Compensate(() => dataDir.Delete(true),
                            retryDelay: TimeSpan.FromSeconds(5));
                        TransientHelper.Compensate(() => DirectoryEx.DirectoryCopyRecursive(tempBackupDir.FullName, dataDir.FullName),
                            retryDelay: TimeSpan.FromSeconds(5));
                        throw;
                    }
                }
            }
        }

        public void DeleteCurrentSettings()
        {
            using (new WurmAssistantGateway(AppContext.WaGatewayErrorMessage))
            {
                var dataDir = new DirectoryInfo(DataDirPath);
                dataDir.Delete(true);
            }
        }

        public IEnumerable<WaBackup> GetBackups()
        {
            var backupsDir = new DirectoryInfo(BackupDirPath);
            var backups = backupsDir.GetFiles("*.7z")
                .Select(x => new WaBackup(x.FullName, BuildType, this, waBackupsViewModel))
                .ToArray();
            return backups;
        }
    }

    public class StableBackupsManager : BackupsManager
    {
        public StableBackupsManager(WaBackupsViewModel waBackupsViewModel) 
            : base(waBackupsViewModel)
        {
            DataDirPath = AppContext.StableDataDir;
            BackupDirPath = AppContext.WaBackupStableDir;
            BuildType = AppContext.StableKey;
        }
    }

    public class BetaBackupsManager : BackupsManager
    {
        public BetaBackupsManager(WaBackupsViewModel waBackupsViewModel)
            : base(waBackupsViewModel)
        {
            DataDirPath = AppContext.BetaDataDir;
            BackupDirPath = AppContext.WaBackupBetaDir;
            BuildType = AppContext.BetaKey;
        }
    }
}
