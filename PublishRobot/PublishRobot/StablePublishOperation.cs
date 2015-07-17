using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Aldurcraft.DevTools
{
    class StablePublishOperation : PublishBaseOperation
    {
        public StablePublishOperation(Robot robot, ArgsManager argsManager) : base(robot, argsManager)
        {
            TargetPublishDir = @"C:\Dropbox\Public\Publish\WurmAssistant2\Stable";
            ValidateDirExists(TargetPublishDir);
        }

        public override void Execute()
        {
            WriteOut("attempting to publish the program");
            WriteOut(string.Format("starting publishing for config: " + BuildConfig));

            Version version = GetVersion();
            DirectoryInfo tempProgDir = CreateTempProgramDir(version);
            DirectoryInfo tempProgDirForOldApi = CreateTempProgramDirForOldApi(version);
            FileInfo zippedProgFile = CreateZippedProgramFile(tempProgDir);
            FileInfo zippedProgFileForOldApi = CreateZippedProgramFile(tempProgDirForOldApi);
            PublishToOldWebApi(zippedProgFileForOldApi);
            PublishToNewWebApi(zippedProgFile, version);
            tempProgDir.Delete(true);
            tempProgDirForOldApi.Delete(true);
            zippedProgFileForOldApi.Delete();
        }
    }
}
