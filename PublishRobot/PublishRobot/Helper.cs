using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aldurcraft.DevTools
{
    static class Helper
    {
        public const string BuildConfigAlphaDebug = "Debug";
        public const string BuildConfigAlphaRelease = "Release";
        public const string BuildConfigBeta = "Beta";
        public const string BuildConfigStable = "Stable";

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, string[] excludeDirList = null, string[] excludeFileTypes = null)
        {
            var dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            if (!Directory.Exists(destDirName))
            {
                Debug.WriteLine("create dir " + destDirName);
                Directory.CreateDirectory(destDirName);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (excludeFileTypes != null && excludeFileTypes.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }
                Debug.WriteLine("copying " + file.FullName);
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    if (excludeDirList == null || !excludeDirList.Contains(subdir.Name))
                    {
                        string temppath = Path.Combine(destDirName, subdir.Name);
                        DirectoryCopy(subdir.FullName, temppath, copySubDirs, excludeDirList);
                    }
                }
            }
        }

        public static void WriteOut(string message)
        {
            string outMessage = string.Format("PublishRobot: {0}", message);
            Console.WriteLine(outMessage);
        }

        public static string GetAssemblyInfoPath(string projDirRoot)
        {
            var assemblyInfoFilePath = Path.Combine(projDirRoot, "Properties", "AssemblyInfo.cs");
            if (!File.Exists(assemblyInfoFilePath))
            {
                throw new RobotException("no assemblyInfo file found, skipping, path=" + assemblyInfoFilePath);
            }
            return assemblyInfoFilePath;
        }

        public static string GetManifestPath(string projDirRoot)
        {
            var manifestFilePath = Path.Combine(projDirRoot, "Properties", "app.manifest");
            if (!File.Exists(manifestFilePath))
            {
                throw new RobotException("no manifest file found, skipping, path=" + manifestFilePath);
            }
            return manifestFilePath;
        }

        public static Version GetVersionFromAssemblyInfo(string projDirRoot)
        {
            string useless;
            return GetVersionFromAssemblyInfoWithFileContents(projDirRoot, out useless);
        }

        public static Version GetVersionFromAssemblyInfoWithFileContents(string projDirRoot, out string fileContents)
        {
            fileContents = File.ReadAllText(GetAssemblyInfoPath(projDirRoot));
            Match match = Regex.Match(fileContents, @"\[assembly: AssemblyVersion\(""(\d+.\d+.\d+.\d+)""\)\]");

            Version vrs;
            if (!Version.TryParse(match.Groups[1].Value, out vrs))
            {
                throw new RobotException("Version failed to parse, skipping release publish");
            }
            return vrs;
        }

        public static string AuthToken 
        {
            get
            {
                return File.ReadAllText(@"D:\_secure\WurmAssistantPublishKey.txt");
            }
        }

        public static bool IsAlphaDebugConfig(string buildConfig)
        {
            return IsStringEqual(buildConfig, BuildConfigAlphaDebug);
        }

        public static bool IsAlphaReleaseConfig(string buildConfig)
        {
            return IsStringEqual(buildConfig, BuildConfigAlphaRelease);
        }

        public static bool IsBetaConfig(string buildConfig)
        {
            return IsStringEqual(buildConfig, BuildConfigBeta);
        }

        public static bool IsStableConfig(string buildConfig)
        {
            return IsStringEqual(buildConfig, BuildConfigStable);
        }

        private static bool IsStringEqual(string a, string b)
        {
            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
