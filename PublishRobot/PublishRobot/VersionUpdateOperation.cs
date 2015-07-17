using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aldurcraft.DevTools
{
    class VersionUpdateOperation : Operation
    {
        private readonly ArgsManager _argsManager;
        private readonly string _projDirRoot;

        private Robot _robot;

        public VersionUpdateOperation(Robot robot, ArgsManager argsManager)
        {
            _robot = robot;
            _argsManager = argsManager;
            _projDirRoot = _argsManager.GetArg(ArgType.ProjDirRoot);
            ValidateDirExists(_projDirRoot);
            BuildConfig = _argsManager.GetArg(ArgType.BuildConfig);
        }

        public override void Execute()
        {
            var assemblyInfoFilePath = Helper.GetAssemblyInfoPath(_projDirRoot);
            var manifestFilePath = Helper.GetManifestPath(_projDirRoot);

            WriteOut("updating version");

            Version newVersion;
            if (ModifyAssemblyInfo(assemblyInfoFilePath, out newVersion))
            {
                ModifyManifest(manifestFilePath, newVersion);
            }
            WriteOut("version update finished");
        }

        bool ModifyAssemblyInfo(string filePath, out Version version)
        {
            WriteOut("modifying AssemblyInfo");
            string assemblyInfoContents;
            version = Helper.GetVersionFromAssemblyInfoWithFileContents(_projDirRoot, out assemblyInfoContents);
            version = new Version(version.Major, version.Minor, version.Build, version.Revision + 1);

            //if (IsAlphaDebugConfig() || IsAlphaReleaseConfig())
            //{
            //    version = new Version(version.Major, version.Minor, version.Build, version.Revision + 1);
            //}
            //else if (IsStableConfig() || IsBetaConfig())
            //{
            //    WriteOut("Publish config detected, skipping version update");
            //    version = null;
            //    return false;
            //}
            //else
            //{
            //    throw new RobotException("BuildConfig type not supported, exiting, BuildConfig == " + BuildConfig);
            //}

            string replacestring = "[assembly: AssemblyVersion(\"" + version + "\")]";
            string replacestring2 = "[assembly: AssemblyFileVersion(\"" + version + "\")]";

            assemblyInfoContents = Regex.Replace(assemblyInfoContents, @"\[assembly: AssemblyVersion\(""(\d+.\d+.\d+.\d+)""\)\]", replacestring);
            assemblyInfoContents = Regex.Replace(assemblyInfoContents, @"\[assembly: AssemblyFileVersion\(""(\d+.\d+.\d+.\d+)""\)\]", replacestring2);

            File.WriteAllText(filePath, assemblyInfoContents, Encoding.UTF8);
            WriteOut("AssemblyInfo modify complete");
            return true;
        }


        void ModifyManifest(string filePath, Version newVersion)
        {
            WriteOut("modifying Manifest");
            string manifest = File.ReadAllText(filePath);

            string replacestring = "assemblyIdentity version=\"" + newVersion + "\"";

            manifest = Regex.Replace(manifest, @"assemblyIdentity version=""\d+.\d+.\d+.\d+""", replacestring);

            File.WriteAllText(filePath, manifest, Encoding.UTF8);
            WriteOut("Manifest modify complete");
        }
    }
}
