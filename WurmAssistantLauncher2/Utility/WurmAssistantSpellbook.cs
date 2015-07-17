using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Aldurcraft.WurmAssistantLauncher2.Utility
{
    public static class WurmAssistantSpellbook
    {
        public static DirectoryInfo GetLatestVersionDirAtLocation(string basePath)
        {
            DirectoryInfo[] dirs = (new DirectoryInfo(basePath).GetDirectories());
            if (dirs.Length == 0) return null;
            return dirs.OrderByDescending(x => GetVersionFromString(x.Name)).First();
        }

        public static bool VersionDirExists(string basePath)
        {
            DirectoryInfo[] dirs = (new DirectoryInfo(basePath).GetDirectories());
            var zeroVersion = new Version();
            return dirs.Any(x => GetVersionFromString(x.Name) != zeroVersion);
        }

        public static Version GetVersionFromString(string str)
        {
            Match match = Regex.Match(str, @"(\d+)_(\d+)_(\d+)_(\d+)");
            if (match.Success)
            {
                return new Version(
                    int.Parse(match.Groups[1].Value), 
                    int.Parse(match.Groups[2].Value), 
                    int.Parse(match.Groups[3].Value), 
                    int.Parse(match.Groups[4].Value));
            }
            else return new Version();
        }
    }
}
