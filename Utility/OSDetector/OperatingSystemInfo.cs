using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aldurcraft.Utility
{
    /// <summary>
    /// Utility intended to detect operating system type, currently discerns: WinXP and Other
    /// </summary>
    /// <remarks>
    /// Many WinForms controls look or work differently in WinXP compared to Win7 and Win8, 
    /// this helps customizing GUI code.
    /// </remarks>
    public static class OperatingSystemInfo
    {
        const string THIS = "OperatingSystemInfo";

        public enum OStype { Unknown, WinXP, Other }

        public static OStype RunningOS { get; private set; }
        public static string RunningOS_Raw { get; private set; }

        static OperatingSystemInfo()
        {
            try
            {
                System.OperatingSystem OS = Environment.OSVersion;
                var platform = Environment.OSVersion.Platform;
                var version = Environment.OSVersion.Version;

                RunningOS_Raw = platform.ToString() + " " + version.ToString();

                if (platform == PlatformID.Win32NT && version.Major == 5 && version.Minor == 1)
                    RunningOS = OStype.WinXP;
                else RunningOS = OStype.Other;

                Logger.LogInfo(String.Format("Detected OS: {0} version {1} interpreted as {2}",
                    platform.ToString(), version.ToString(), RunningOS.ToString()), THIS);
            }
            catch (Exception _e)
            {
                Logger.LogInfo("problem detecting operating system", THIS, _e);
                RunningOS = OStype.Unknown;
            }
        }
    }
}
