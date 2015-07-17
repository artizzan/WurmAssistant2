using System;
using System.IO;

namespace Aldurcraft.DevTools
{
    internal class AlphaReleasePublishOperation : PublishBaseOperation
    {
        public AlphaReleasePublishOperation(Robot robot, ArgsManager argsManager)
            : base(robot, argsManager)
        {
            TargetPublishDir = @"E:\Dropbox\ProjectsPublished\WurmAssistant2\Alpha";
            ValidateDirExists(TargetPublishDir);
        }

        public override void Execute()
        {
            WriteOut("attempting to publish the program");
            WriteOut(string.Format("starting publishing for config: "+BuildConfig));

            Version version = GetVersion();
            DirectoryInfo tempProgDir = CreateTempProgramDir(version);
            CreateZippedProgramFile(tempProgDir);
            tempProgDir.Delete(true);
        }
    }
}