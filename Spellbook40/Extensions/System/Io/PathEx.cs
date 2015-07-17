using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Aldurcraft.Spellbook40.Extensions.System.Reflection;

namespace Aldurcraft.Spellbook40.Extensions.System.IO
{
    public static class PathEx
    {
        public static string CombineWithCodeBase(params string[] paths)
        {
            var allArgs = new string[paths.Length + 1];
            allArgs[0] = AssemblyEx.CodeBaseLocalDirPath;
            for (int i = 0; i < paths.Length; i++)
            {
                allArgs[i + 1] = paths[i];
            }
            return Path.Combine(allArgs);
        }

        public static bool HasInvalidFileCharacters(string filePath)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return filePath.IndexOfAny(invalidChars) > -1;
        }
    }
}
