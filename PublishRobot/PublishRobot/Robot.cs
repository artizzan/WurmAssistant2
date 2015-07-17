using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aldurcraft.DevTools
{
    class Robot
    {
        readonly ArgsManager argsManager;

        public Robot(IEnumerable<string> args)
        {
            argsManager = new ArgsManager(args);
        }

        public void Execute()
        {
            Helper.WriteOut("getting commands");
            var commands = argsManager.GetArgs(ArgType.Command);
            Helper.WriteOut("creating operations");
            var operations = commands.Select(GetOperation).ToList();
            Helper.WriteOut("executing operations");
            operations.ForEach(operation =>
            {
                if (operation != null) operation.Execute();
            } );
            Helper.WriteOut("done");
        }

        private Operation GetOperation(string commandText)
        {
            Helper.WriteOut("Trying to create operation for: " + commandText);

            if (IsCommand(commandText, "VersionUpdate"))
            {
                return new VersionUpdateOperation(this, argsManager);
            }
            if (IsCommand(commandText, "Publish"))
            {
                var buildConfig = argsManager.GetArg(ArgType.BuildConfig);
                if (Helper.IsAlphaDebugConfig(buildConfig))
                {
                    Helper.WriteOut("No publishing configured for " + buildConfig);
                    return null;
                }
                if (Helper.IsAlphaReleaseConfig(buildConfig))
                {
                    return new AlphaReleasePublishOperation(this, argsManager);
                }
                if (Helper.IsBetaConfig(buildConfig))
                {
                    return new BetaPublishOperation(this, argsManager);
                }
                if (Helper.IsStableConfig(buildConfig))
                {
                    return new StablePublishOperation(this, argsManager);
                }
                throw new RobotException("Error: Unknown build config!");
            }
            if (IsCommand(commandText, "UpdateChangelog"))
            {
                return new UpdateChangelogOperation(this, argsManager);
            }
            throw new RobotException("Unrecognized command: " + commandText);
        }

        private bool IsCommand(string commandText, string commandTemplate)
        {
            return (string.Compare(commandText, commandTemplate, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}
