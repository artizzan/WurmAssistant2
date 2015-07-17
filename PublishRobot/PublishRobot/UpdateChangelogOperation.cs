using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aldurcraft.DevTools
{
    class UpdateChangelogOperation : Operation
    {
        private readonly Robot robot;
        private readonly ArgsManager argsManager;
        private readonly string projDirRoot;

        public UpdateChangelogOperation(Robot robot, ArgsManager argsManager)
        {
            this.robot = robot;
            this.argsManager = argsManager;
            BuildConfig = argsManager.GetArg(ArgType.BuildConfig);
            projDirRoot = argsManager.GetArg(ArgType.ProjDirRoot);
            ValidateDirExists(projDirRoot);
        }

        public override void Execute()
        {
            //intended to be applied prebuild, after any version updates
            WriteOut("Attempting to update changelog");

            try
            {
                Version vrs = Helper.GetVersionFromAssemblyInfo(projDirRoot);

                string newAddsPath = Path.Combine(projDirRoot, "CHANGELOG_NEWADDS.txt");
                ValidateFileExists(newAddsPath);

                string changelogPath = Path.Combine(projDirRoot, "CHANGELOG.txt");
                if (!File.Exists(changelogPath))
                {
                    File.WriteAllText(changelogPath, string.Empty);
                }

                var newAddsContents = File.ReadAllLines(newAddsPath);

                string prependContents = string.Empty;
                foreach (var line in newAddsContents)
                {
                    var trimmedLine = line.Trim();
                    if (!trimmedLine.StartsWith("#") && trimmedLine != string.Empty)
                    {
                        prependContents += trimmedLine + "\r\n";
                    }
                }
                prependContents = prependContents.Trim();

                if (prependContents == string.Empty)
                {
                    WriteOut("No changes detected for changelog, skipping");
                    return;
                }

                string headerVersion = vrs.ToString();
                string headerStamp = DateTime.Now.ToString("dd-MM-yyyy H:mm:ss");

                var changelogContents = File.ReadAllText(changelogPath);
                changelogContents = string.Format("{0} ({1})", headerVersion, headerStamp) + "\r\n" + prependContents + "\r\n" + "\r\n" + changelogContents;

                File.WriteAllText(changelogPath, changelogContents);
                File.WriteAllText(newAddsPath, "# anything added below this comment line will be appended to changelog on next non-debug build\r\n");

                //finish
                WriteOut("Changelog Updated");
            }
            catch (Exception exception)
            {
                throw new RobotException(string.Format("Error while attempting to update changelog, {0}\r\n{1}",
                    exception.Message, exception.StackTrace));
            }
        }
    }
}
