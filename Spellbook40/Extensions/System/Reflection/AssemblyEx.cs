using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Aldurcraft.Spellbook40.Extensions.System.Reflection
{
    public static class AssemblyEx
    {
        private static string codeBaseLocalDirPath;
        public static string CodeBaseLocalDirPath
        {
            get
            {
                if (codeBaseLocalDirPath == null)
                {
                    var x = (new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath;
                    x = Path.GetDirectoryName(x);
                    codeBaseLocalDirPath = x;
                }
                return codeBaseLocalDirPath;
            }
        }

        private static Version entryAssemblyVersion;
        public static Version EntryAssemblyVersion
        {
            get
            {
                if (entryAssemblyVersion == null)
                {
                    entryAssemblyVersion = Assembly.GetEntryAssembly().GetName().Version;
                }
                return entryAssemblyVersion;
            }
        }
    }
}
