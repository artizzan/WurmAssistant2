using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aldurcraft.DevTools
{
    enum RobotExitCode
    {
        Default = 0,
        ArgumentMissing = 1,
        InvalidArgument = 2,
        ErrorEncountered = 3,
        InvalidPath = 4,
        NullCommand = 5,
        UnrecognizedCommand = 6,
        UpdateVersion_AssemblyInfoMissing = 7,
        UpdateVersion_ManifestMissing = 8,
        UpdateVersion_BuildConfigNotSupported = 9,
        ReleasePublish_BuildPathDoesNotExist = 10,
        ReleasePublish_VersionFailedToParse = 11,
        UpdateChangelog_NoNewAddsFileDetected = 12,
        UpdateChangelog_ErrorUpdatingChangelog = 13,
        UpdateChangelog_VersionFailedToParse = 14
    }

    class RobotException : Exception
    {
        public RobotException(string message, RobotExitCode exitCode = 0) : base(message)
        {
            ExitCode = exitCode;
        }

        public RobotExitCode ExitCode { get; private set; }
    }
}
