using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Aldurcraft.Spellbook40.Extensions.System;
using Aldurcraft.Spellbook40.Extensions.System.IO;
using Aldurcraft.Spellbook40.Transient;
using Aldurcraft.Spellbook40.WPF.Toolkit.Commands;
using Aldurcraft.WurmAssistantLauncher2.Models;
using Aldurcraft.WurmAssistantLauncher2.ViewModels;
using Aldurcraft.WurmAssistantMutexes;
using SevenZip;
using WurmAssistantLauncher2.Annotations;

namespace Aldurcraft.WurmAssistantLauncher2.Managers
{
    public class WaBackup : INotifyPropertyChanged
    {
        public DateTime TimeStamp
        {
            get { return timeStamp; }
            private set
            {
                if (value.Equals(timeStamp)) return;
                timeStamp = value;
                OnPropertyChanged();
            }
        }

        public string FilePath
        {
            get { return filePath; }
            private set
            {
                if (value == filePath) return;
                filePath = value;
                OnPropertyChanged();
            }
        }

        public string RawBackupName
        {
            get { return rawBackupName; }
            private set
            {
                if (value == rawBackupName) return;
                rawBackupName = value;
                OnPropertyChanged();
            }
        }

        public string Comment
        {
            get { return comment; }
            private set
            {
                if (value == comment) return;
                comment = value;
                OnPropertyChanged();
            }
        }
        public string BackupName
        {
            get { return backupName; }
            set
            {
                try
                {
                    if (value == backupName) return;
                    Rename(value);
                }
                catch (Exception exception)
                {
                    ErrorManager.ShowWarning(exception.Message, exception);
                }
                OnPropertyChanged();
            }
        }

        private bool isDeleted = false;

        private readonly BackupsManager manager;
        private readonly WaBackupsViewModel baseViewModel;
        private DateTime timeStamp;
        private string filePath;
        private string rawBackupName;
        private string backupName;
        private string comment;

        public WaBackup(string filePath, string verificationBuildType, BackupsManager manager, WaBackupsViewModel baseViewModel)
        {
            this.manager = manager;
            this.baseViewModel = baseViewModel;
            FilePath = filePath;
            SetNames(verificationBuildType);

            RestoreCommand = new DelegateCommand(() =>
            {
                try
                {
                    Restore();
                }
                catch (Exception exception)
                {
                    ErrorManager.ShowWarning(exception.Message, exception);
                }
            });

            DeleteCommand = new DelegateCommand(() =>
            {
                try
                {
                    Delete();
                }
                catch (Exception exception)
                {
                    ErrorManager.ShowWarning(exception.Message, exception);
                }
            });
        }

        public DelegateCommand RestoreCommand { get; private set; }
        public DelegateCommand DeleteCommand { get; private set; }

        private void MoveInto(string targetDirPath)
        {
            ValidateNotDeleted();

            var file = new FileInfo(FilePath);
            var targetFilePath = Path.Combine(targetDirPath, file.Name);

            ValidateTargetNotExists(targetFilePath);

            file.MoveTo(targetFilePath);
            FilePath = file.FullName;
        }

        private void ValidateTargetNotExists(string targetFilePath)
        {
            if (File.Exists(targetFilePath))
            {
                throw new LauncherException("backup with this file name already exists");
            }
        }

        private void SetNames(string verificationBuildType = null)
        {
            var backupFile = new FileInfo(FilePath);
            Match match = Regex.Match(backupFile.Name, @"(.+)_(\w+)_(\d\d\d\d-\d\d-\d\d__\d\d-\d\d-\d\d)_\d+\.7z");
            if (match.Success)
            {
                RawBackupName = backupName = match.Groups[1].Value;

                if (verificationBuildType != null)
                {
                    if (
                        !String.Equals(verificationBuildType, match.Groups[2].Value,
                            StringComparison.InvariantCultureIgnoreCase))
                    {
                        backupName += " (!)";
                        Comment = "this file has mis-matched build type: " + match.Groups[2].Value;
                        App.Logger.LogInfo("Backup file name incorrect build type, " + FilePath);
                    }
                }

                DateTime dt;
                if (DateTimeEx.TryGetDateTimeFromNowString(match.Groups[3].Value, out dt))
                {
                    TimeStamp = dt;
                }
            }
            else
            {
                backupName = backupFile.Name + " (!)";
                Comment = "this file has invalid file name format";
                App.Logger.LogInfo("Backup file name incorrect format, " + FilePath);
            }
        }

        private string GetRenamedFilePath(string newName)
        {
            var file = new FileInfo(FilePath);
            var oldFilename = file.Name;
            var result = Regex.Replace(file.Name, @".+(_\w+_\d\d\d\d-\d\d-\d\d__\d\d-\d\d-\d\d_\d+\.7z)",
                newName + @"$1");
            if (oldFilename != result)
            {
                return Path.Combine(file.DirectoryName, result);
            }
            else
            {
                throw new LauncherException("backup rename failed! filepath: " + file.FullName);
            }
        }

        public void Restore()
        {
            using (new WurmAssistantGateway(AppContext.WaGatewayErrorMessage))
            {
                using (var tempDirMan = new TempDirManager())
                {
                    var tempDir = tempDirMan.GetHandle();
                    var dataDir = new DirectoryInfo(manager.DataDirPath);

                    var tempBackupDir = new DirectoryInfo(Path.Combine(tempDir.FullName, dataDir.Name));
                    DirectoryEx.DirectoryCopyRecursive(dataDir.FullName, tempBackupDir.FullName);

                    try
                    {
                        dataDir.Delete(true);
                        ValidateNotDeleted();

                        using (var extractor = new SevenZipExtractor(FilePath))
                        {
                            extractor.ExtractArchive(dataDir.FullName);
                        }
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

        private void Rename(string newName)
        {
            ValidateNotDeleted();

            ValidateName(newName);

            var backupFile = new FileInfo(FilePath);
            var newFilePath = GetRenamedFilePath(newName);
            if (File.Exists(newFilePath))
            {
                throw new LauncherException("file with this name already exists! " + newFilePath);
            }
            backupFile.MoveTo(newFilePath);

            FilePath = newFilePath;
            SetNames();
        }

        public void Delete()
        {
            File.Delete(FilePath);
            isDeleted = true;
            baseViewModel.WaBackups.Remove(this);
        }

        private void ValidateNotDeleted()
        {
            if (isDeleted)
            {
                throw new InvalidOperationException("this backup has been deleted");
            }
        }

        public static WaBackup Create(string backupName, string buildType, string sourceDirPath, string targetDirPath, BackupsManager manager, WaBackupsViewModel baseViewModel)
        {
            ValidateName(backupName);

            using (new WurmAssistantGateway(AppContext.WaGatewayErrorMessage))
            {
                using (var tempDirMan = new TempDirManager())
                {
                    var tempDir = tempDirMan.GetHandle();

                    string tempArchiveFilePath = Path.Combine(
                        tempDir.FullName,
                        string.Format("{0}_{1}_{2}.7z",
                            backupName ?? "WurmAssistantBackup",
                            buildType,
                            DateTime.Now.ToStringFormattedForFileEx(highPrecision: true)));

                    var compressor = new SevenZipCompressor()
                    {
                        ArchiveFormat = OutArchiveFormat.SevenZip,
                        CompressionMode = CompressionMode.Create
                    };
                    compressor.CompressDirectory(sourceDirPath, tempArchiveFilePath);

                    var newBackup = new WaBackup(tempArchiveFilePath, buildType, manager, baseViewModel);

                    var targetFile = new FileInfo(newBackup.FilePath);
                    var targetArchiveFile = new FileInfo(Path.Combine(targetDirPath, targetFile.Name));

                    if (targetArchiveFile.Exists)
                    {
                        throw new LauncherException("backup with this file name already exists");
                    }

                    newBackup.MoveInto(targetDirPath);

                    return newBackup;
                }
            }
        }

        public static void ValidateName(string newBackupName)
        {
            if (newBackupName != null)
            {
                newBackupName = newBackupName.Trim();
                if (newBackupName == string.Empty)
                {
                    throw new LauncherException("backup name cannot be empty");
                }

                if (PathEx.HasInvalidFileCharacters(newBackupName))
                {
                    throw new LauncherException("backup name cannot contain invalid file name characters");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
