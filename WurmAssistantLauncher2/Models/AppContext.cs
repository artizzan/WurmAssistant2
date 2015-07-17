using System;
using System.IO;
using System.Reflection;
using Aldurcraft.Spellbook40.Extensions.System.Reflection;

namespace Aldurcraft.WurmAssistantLauncher2.Models
{
    public enum BuildType { Unknown = 0, Beta, Stable }
    public static class AppContext
    {
        public static string BetaKey { get { return "Beta"; } }
        public static string StableKey { get { return "Stable"; } }
        public static string LocalAppData { get; private set; }
        //localappdata/Aldurcraft/WurmAssistantData/Beta
        public static string BetaDataDir { get; private set; }
        //localappdata/Aldurcraft/WurmAssistantData/Stable
        public static string StableDataDir { get; private set; }
        //localappdata/Aldurcraft/WurmAssistantLauncher/Settings
        public static string LauncherSettingsDir { get; private set; }
        //localappdata/Aldurcraft/WurmAssistantLauncher/Backups/Beta
        public static string WaBackupBetaDir { get; private set; }
        //localappdata/Aldurcraft/WurmAssistantLauncher/Backups/Stable
        public static string WaBackupStableDir { get; private set; }
        //localappdata/Aldurcraft/WurmAssistantInstall/Beta
        public static string BetaInstallDir { get; private set; }
        //localappdata/Aldurcraft/WurmAssistantInstall/Stable
        public static string StableInstallDir { get; private set; }
        public static string TempDir { get; private set; }
        public static string OldDataDir { get; private set; }

        public static string WebApiBasePath { get; private set; }
        public static string ProjectName { get; private set; }
        public static Version ProgramVersion { get; private set; }

        public const string WaGatewayErrorMessage = "Some other operation is under way or Wurm Assistant needs to be closed.";

        public static void BuildContext(string rootOverride = null, string webApiOverride = null)
        {
            ProjectName = "WurmAssistant";

            ProgramVersion = Assembly.GetExecutingAssembly().GetName().Version;

            // params for testing
#if DEBUG
            rootOverride = AssemblyEx.CodeBaseLocalDirPath;
            webApiOverride = @"http://localhost:19296/";
#endif
            LocalAppData = rootOverride ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            WebApiBasePath = webApiOverride ?? @"http://aldurcraft.com/";

            BetaDataDir = Path.Combine(LocalAppData, "Aldurcraft", "WurmAssistantData", BetaKey);
            StableDataDir = Path.Combine(LocalAppData, "Aldurcraft", "WurmAssistantData", StableKey);
            LauncherSettingsDir = Path.Combine(LocalAppData, "Aldurcraft", "WurmAssistantLauncher", "Settings");
            WaBackupBetaDir = Path.Combine(LocalAppData, "Aldurcraft", "WurmAssistantLauncher", "Backups", BetaKey);
            WaBackupStableDir = Path.Combine(LocalAppData, "Aldurcraft", "WurmAssistantLauncher", "Backups", StableKey);
            BetaInstallDir = Path.Combine(LocalAppData, "Aldurcraft", "WurmAssistantInstall", BetaKey);
            StableInstallDir = Path.Combine(LocalAppData, "Aldurcraft", "WurmAssistantInstall", StableKey);
            TempDir = Path.Combine(LocalAppData, "Aldurcraft", "WurmAssistantLauncher", "Temp");
            OldDataDir = Path.Combine(LocalAppData, "AldurCraft", "WurmAssistant2");
        }

        public static string GetDataDir(BuildType buildType)
        {
            switch (buildType)
            {
                case BuildType.Beta:
                    return BetaDataDir;
                case BuildType.Stable:
                    return StableDataDir;
                default:
                    throw new LauncherException("unknown build type for GetDataDir, " + buildType);
            }
        }
    }
}
