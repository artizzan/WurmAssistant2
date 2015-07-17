using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aldurcraft.Spellbook;
using Aldurcraft.WurmAssistantMutexes;
using SevenZip;
using WurmAssistantLauncher2.Models;
using WurmAssistantLauncher2.Utility;

namespace WurmAssistantLauncher2.Operators
{
    class Updater
    {
        // run under WA mutex
        protected string BasePath;
        protected string buildType;

        protected Updater()
        {
        }

        public bool IsUpdateAvailable()
        {
            // ask web service if available
            var latestRemote = WebApiClientSpellbook.GetObjectFromWebApi<Version>(
                AppContext.WebApiBasePath,
                string.Format("LatestVersion/{0}/{1}", AppContext.ProjectName, buildType),
                TimeSpan.FromSeconds(15));
            var latestLocalDir = WurmAssistantSpellbook.GetLatestVersionDirAtLocation(BasePath);
            var latestLocal = WurmAssistantSpellbook.GetVersionFromString(latestLocalDir.Name);

            return latestLocal < latestRemote;
        }

        public void Update()
        {
            if (!IsUpdateAvailable())
            {
                throw new LauncherException("no update is available");
            }

            using (var mutex = new WurmAssistantMutex())
            {
                mutex.Enter(1000, "You must close Wurm Assistant before running update");
                var newFileZipped = WebApiClientSpellbook.GetFileFromWebApi(
                    AppContext.WebApiBasePath,
                    string.Format("{0}/{1}", AppContext.ProjectName, buildType),
                    TimeSpan.FromSeconds(15),
                    BasePath);

                using (var extractor = new SevenZipExtractor(newFileZipped.FullName))
                {
                    extractor.ExtractArchive(BasePath);
                }

                Thread.Sleep(1000);

                var cleaner = new DirCleaner(BasePath);
                cleaner.Cleanup();
            }
        }
        class DirCleaner
        {
            private string cleanupDir;
            public DirCleaner(string dir)
            {
                cleanupDir = dir;
            }

            //clean up the dir, leaving only the latest version unzipped directory
            public void Cleanup()
            {
                if (string.IsNullOrWhiteSpace(cleanupDir)) throw new LauncherException("cleanup dir path is invalid");

                var cleanupDirInfo = new DirectoryInfo(cleanupDir);
                var latestDir = WurmAssistantSpellbook.GetLatestVersionDirAtLocation(cleanupDirInfo.FullName);
                latestDir
                    .GetFiles()
                    .ToList()
                    .ForEach(x => x.Delete());
                latestDir
                    .GetDirectories()
                    .Where(x => x.FullName != latestDir.FullName)
                    .ToList()
                    .ForEach(x => x.Delete(true));
            }
        }
    }


    class BetaUpdater : Updater
    {
        public BetaUpdater()
        {
            BasePath = AppContext.BetaInstallDir;
            buildType = AppContext.BetaKey;
        }
    }

    class StableUpdater : Updater
    {
        public StableUpdater()
        {
            BasePath = AppContext.StableInstallDir;
            buildType = AppContext.StableKey;
        }
    }
}
