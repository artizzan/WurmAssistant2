using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aldurcraft.Spellbook40.Io;

namespace Aldurcraft.Spellbook40.Extensions.System.IO
{
    public static class DirectoryEx
    {
        /// <summary>
        /// Copies directory, by default includes all subdirectories recursively. 
        /// Can optionally use a whitelist or blacklist (full dir paths).
        /// Throws exceptions on any copy errors. NOT transactional!
        /// White and blacklists ignore case, this may not be suitable for Linux!
        /// </summary>
        /// <remarks>
        /// Blacklisting works as expected, if any subdir matches full path of any blacklisted entry,
        /// that subdir and any lower-level subdirs are ignored.
        /// Whitelisting also works as expected, meaning it will copy any whitelisted subdirectory,
        /// regardless how deep it is nested. It will also avoid creating empty directory chains,
        /// if given tree branch has no whitelisted subdirs.
        /// Algorithm is unable to blacklist or whitelist individual files.
        /// Top-level directory files are always copied, regardless of above lists.
        /// </remarks>
        /// <param name="sourceDirName">source full path</param>
        /// <param name="destDirName">destination full path</param>
        /// <param name="copySubDirs">true to copy all subdirs, false to copy only top dir</param>
        /// <param name="whitelist">list of full subdir paths to copy recursively, all other are ignored</param>
        /// <param name="blacklist">list of full subdir paths to not copy recursively, all other are copied</param>
        /// <exception cref="InvalidOperationException">whitelist and blacklist can't be used simultaneously</exception>
        public static void DirectoryCopyAdv(string sourceDirName, string destDirName, bool copySubDirs = true, string[] whitelist = null, string[] blacklist = null)
        {
            DirectoryCopier copier = new DirectoryCopier();
            copier.DirectoryCopy(sourceDirName, destDirName, copySubDirs, whitelist, blacklist);
        }

        /// <summary>
        /// Copies directory with all subdirectories recursively. Exception on failure, no rollback!
        /// </summary>
        /// <param name="sourceDirName">full path</param>
        /// <param name="destDirName">full path</param>
        public static void DirectoryCopyRecursive(string sourceDirName, string destDirName)
        {
            DirectoryCopyAdv(sourceDirName, destDirName);
        }

        public static void ClearDirectory(string dirPath)
        {
            var clearDir = new DirectoryInfo(dirPath);
            clearDir.GetDirectories().ToList().ForEach(x => x.Delete(true));
            clearDir.GetFiles().ToList().ForEach(x => x.Delete());
        }
    }
}
