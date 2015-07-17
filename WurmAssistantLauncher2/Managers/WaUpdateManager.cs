using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aldurcraft.Spellbook40.Transient;
using Aldurcraft.Spellbook40.WebApi;
using Aldurcraft.WurmAssistantLauncher2.Models;
using Aldurcraft.WurmAssistantLauncher2.Utility;
using Aldurcraft.WurmAssistantLauncher2.ViewModels;
using Aldurcraft.WurmAssistantMutexes;
using SevenZip;

namespace Aldurcraft.WurmAssistantLauncher2.Managers
{
    public class WaUpdateManager
    {
        public event EventHandler<UpdateStatusEventArgs> UpdateStatusChanged;

        protected string BasePath;
        public string BuildType { get; protected set; }

        protected WaUpdateManager()
        {
        }

        #region WA UPDATE

        private bool updateRunning;
        private string updateStatus = string.Empty;
        private int updateProgress;
        private bool hasError;
        private bool progressIndeterminate = true;

        public bool UpdateRunning
        {
            get { return updateRunning; }
            private set
            {
                if (updateRunning != value)
                {
                    updateRunning = value;
                    OnUpdateStatusChanged();
                }
            }
        }

        public string UpdateStatus
        {
            get { return updateStatus; }
            private set
            {
                if (updateStatus != value)
                {
                    updateStatus = value;
                    OnUpdateStatusChanged();
                }
            }
        }

        public int UpdateProgress
        {
            get { return updateProgress; }
            private set
            {
                if (updateProgress != value)
                {
                    updateProgress = value;
                    OnUpdateStatusChanged();
                }
            }
        }

        public bool ProgressIndeterminate
        {
            get { return progressIndeterminate; }
            set
            {
                if (progressIndeterminate != value)
                {
                    progressIndeterminate = value;
                    OnUpdateStatusChanged();
                }
            }
        }

        void OnUpdateStatusChanged()
        {
            var eh = UpdateStatusChanged;
            if (eh != null)
            {
                eh(this, new UpdateStatusEventArgs(
                    updateRunning, updateStatus, updateProgress, hasError, ProgressIndeterminate));
            }
        }

        public void StatusReset()
        {
            UpdateRunning = false;
            UpdateProgress = 0;
            UpdateStatus = string.Empty;
            ProgressIndeterminate = true;
        }

        public async Task UpdateAsync()
        {
            StatusReset();
            UpdateRunning = true;
            UpdateStatus = "starting update";

            var updateIsReallyAvailable = await IsUpdateAvailableAsync();
            if (!updateIsReallyAvailable)
            {
                // this should never happen under normal circumstances
                UpdateStatus = "no update available!";
                throw new LauncherException("no update available");
            }

            using (new WurmAssistantGateway(AppContext.WaGatewayErrorMessage))
            {
                // clean up the dir before trying the update
                updateStatus = "preparing for update";
                await TaskEx.Run(() =>
                {
                    var cleaner = new DirCleaner(BasePath);
                    cleaner.Cleanup();
                });

                updateStatus = "downloading";
                ProgressIndeterminate = false;
                var newFileZipped = await WebApiEx.GetFileFromWebApiAsync(
                    AppContext.WebApiBasePath,
                    string.Format("ProjectApi/{0}/{1}", AppContext.ProjectName, BuildType),
                    TimeSpan.FromSeconds(15),
                    Path.Combine(BasePath, Guid.NewGuid().ToString() + ".zip"),
                    (sender, args) =>
                    {
                        var prog = ((double)args.BytesReceived) / ((double)args.TotalBytesToReceive);
                        prog *= 1000;
                        var intProg = (int)prog;
                        UpdateProgress = intProg;
                    });
                ProgressIndeterminate = true;
                UpdateStatus = "extracting";
                await TaskEx.Run(() =>
                {
                    using (var extractor = new SevenZipExtractor(newFileZipped.FullName))
                    {
                        extractor.ExtractArchive(BasePath);
                    }
                });
                UpdateStatus = "cleaning up";
                await TaskEx.Delay(TimeSpan.FromSeconds(1));

                await TaskEx.Run(() =>
                {
                    var cleaner = new DirCleaner(BasePath);
                    cleaner.Cleanup();
                });
                UpdateStatus = "done";

                await TaskEx.Delay(TimeSpan.FromSeconds(3));
                UpdateRunning = false;
            }
        }
        #endregion

        public async Task UninstallAsync()
        {
            using (new WurmAssistantGateway(AppContext.WaGatewayErrorMessage))
            {
                await Task.Factory.StartNew(() =>
                {
                    var cleaner = new DirCleaner(BasePath);
                    cleaner.WipeDir();
                });
            }
        }

        public async Task<bool> IsUpdateAvailableAsync()
        {
            // ask web service if available
            var resultString = await TransientHelper.CompensateAsync(
                () => WebApiEx.GetObjectFromWebApiAsync<string>(
                    AppContext.WebApiBasePath,
                    string.Format("ProjectApi/LatestVersion/{0}/{1}", AppContext.ProjectName, BuildType),
                    TimeSpan.FromSeconds(15)),
                "Failed to check latest WA version for build type " + BuildType,
                5);
            var latestRemote = new Version(resultString);
            var latestLocalDir = WurmAssistantSpellbook.GetLatestVersionDirAtLocation(BasePath);
            var latestLocal = latestLocalDir == null
                ? new Version()
                : WurmAssistantSpellbook.GetVersionFromString(latestLocalDir.Name);

            return latestLocal < latestRemote;
        }

        class DirCleaner
        {
            private readonly string cleanupDir;
            public DirCleaner(string dir)
            {
                cleanupDir = dir;
            }

            //clean up the dir, leaving only the latest version unzipped directory
            public void Cleanup()
            {
                if (string.IsNullOrWhiteSpace(cleanupDir))
                {
                    throw new LauncherException("cleanup dir path is invalid");
                }

                var cleanupDirInfo = new DirectoryInfo(cleanupDir);
                var latestDir = WurmAssistantSpellbook.GetLatestVersionDirAtLocation(
                    cleanupDirInfo.FullName);
                cleanupDirInfo
                    .GetFiles()
                    .ToList()
                    .ForEach(x => x.Delete());
                cleanupDirInfo
                    .GetDirectories()
                    .Where(x => x.FullName != latestDir.FullName)
                    .ToList()
                    .ForEach(x => x.Delete(true));
            }

            public void WipeDir()
            {
                if (string.IsNullOrWhiteSpace(cleanupDir))
                {
                    throw new LauncherException("cleanup dir path is invalid");
                }

                var cleanupDirInfo = new DirectoryInfo(cleanupDir);
                cleanupDirInfo
                    .GetFiles()
                    .ToList()
                    .ForEach(x => x.Delete());
                cleanupDirInfo
                    .GetDirectories()
                    .ToList()
                    .ForEach(x => x.Delete(true));
            }
        }

        public class UpdateStatusEventArgs : EventArgs
        {
            public bool IsRunning { get; private set; }
            public string Status { get; private set; }
            public int Progress { get; private set; }
            public bool HasError { get; private set; }
            public bool ProgressIndeterminate { get; private set; }

            public UpdateStatusEventArgs(bool isRunning, string status, 
                int progress, bool hasError, bool progressIndeterminate)
            {
                Status = status;
                Progress = progress;
                HasError = hasError;
                ProgressIndeterminate = progressIndeterminate;
                IsRunning = isRunning;
            }
        }
    }


    public class BetaWaUpdateManager : WaUpdateManager
    {
        public BetaWaUpdateManager()
        {
            BasePath = AppContext.BetaInstallDir;
            BuildType = AppContext.BetaKey;
        }
    }

    public class StableWaUpdateManager : WaUpdateManager
    {
        public StableWaUpdateManager()
        {
            BasePath = AppContext.StableInstallDir;
            BuildType = AppContext.StableKey;
        }
    }
}
