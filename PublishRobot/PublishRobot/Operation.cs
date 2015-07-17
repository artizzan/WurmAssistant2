using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aldurcraft.DevTools
{
    abstract class Operation
    {
        protected string BuildConfig;

        public abstract void Execute();

        protected void WriteOut(string text)
        {
            Helper.WriteOut(text);
        }

        protected void ValidateDirExists(string dirPath)
        {
            if (!Directory.Exists(dirPath)) throw new RobotException("directory does not exist: " + dirPath);
        }

        protected void ValidateFileExists(string filePath)
        {
            if (!File.Exists(filePath)) throw new RobotException("file does not exist: " + filePath);
        }

        protected bool IsAlphaDebugConfig()
        {
            return Helper.IsAlphaDebugConfig(BuildConfig);
        }

        protected bool IsAlphaReleaseConfig()
        {
            return Helper.IsAlphaReleaseConfig(BuildConfig);
        }

        protected bool IsBetaConfig()
        {
            return Helper.IsBetaConfig(BuildConfig);
        }

        protected bool IsPublishConfig()
        {
            return Helper.IsStableConfig(BuildConfig);
        }
    }
}
