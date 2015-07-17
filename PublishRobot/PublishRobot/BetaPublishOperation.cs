using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aldurcraft.DevTools
{
    class BetaPublishOperation : PublishBaseOperation
    {
        public BetaPublishOperation(Robot robot, ArgsManager argsManager) : base(robot, argsManager)
        {
            TargetPublishDir = @"D:\Dropbox\Public\Publish\WurmAssistant2\Beta";
            ValidateDirExists(TargetPublishDir);
        }

        public override void Execute()
        {
            WriteOut("attempting to publish the program");
            WriteOut(string.Format("starting publishing for config: " + BuildConfig));

            Version version = GetVersion();
            DirectoryInfo tempProgDir = CreateTempProgramDir(version);
            FileInfo zippedProgFile = CreateZippedProgramFile(tempProgDir);
            PublishToNewWebApi(zippedProgFile, version);
            tempProgDir.Delete(true);
        }
    }
}
