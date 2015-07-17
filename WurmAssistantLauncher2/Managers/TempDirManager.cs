using System;
using System.IO;
using Aldurcraft.WurmAssistantLauncher2.Models;

namespace Aldurcraft.WurmAssistantLauncher2.Managers
{
    class TempDirManager : IDisposable
    {
        // handle temporary, GUID-named subdir under launcher temp dir
        // methods for creating, clearing
        private readonly DirectoryInfo tempDirectoryInfo;
        public TempDirManager()
        {
            // create temp dir
            tempDirectoryInfo = new DirectoryInfo(Path.Combine(AppContext.TempDir, Guid.NewGuid().ToString()));
            if (tempDirectoryInfo.Exists)
            {
                throw new LauncherException("attempt to create temporary directory failed, because directory with this name already exists");
            }
            tempDirectoryInfo.Create();
        }

        public DirectoryInfo GetHandle()
        {
            return new DirectoryInfo(tempDirectoryInfo.FullName);
        }

        public void Dispose()
        {
            tempDirectoryInfo.Delete(true);
        }
    }
}
