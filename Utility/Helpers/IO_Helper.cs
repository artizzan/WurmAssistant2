using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Aldurcraft.Utility
{
    /// <summary>
    /// Utilities intended to help with System.IO and related functionality.
    /// </summary>
    public static class IO_Helper
    {
        public class StringWriterFlushEventArgs : EventArgs
        {
            public readonly string Value;

            public StringWriterFlushEventArgs(string value)
            {
                this.Value = value;
            }
        }

        /// <summary>
        /// StringWriter with auto-flushing capability.
        /// </summary>
        public class StringWriterFlushEvent : StringWriter
        {
            /// <summary>
            /// Triggers when flush occours and provides text that was flushed
            /// </summary>
            public event EventHandler<StringWriterFlushEventArgs> Flushed;
            public virtual bool AutoFlush { get; set; }

            public StringWriterFlushEvent()
                : base() { }

            public StringWriterFlushEvent(bool autoFlush)
                : base() { this.AutoFlush = autoFlush; }

            protected void OnFlush()
            {
                var eh = Flushed; //tsafe
                if (eh != null)
                {
                    string newText = this.GetStringBuilder().ToString();
                    eh(this, new StringWriterFlushEventArgs(newText));
                }
                this.GetStringBuilder().Clear();
            }

            public override void Flush()
            {
                base.Flush();
                OnFlush();
            }

            public override void Write(char value)
            {
                base.Write(value);
                if (AutoFlush) Flush();
            }

            public override void Write(string value)
            {
                base.Write(value);
                if (AutoFlush) Flush();
            }

            public override void Write(char[] buffer, int index, int count)
            {
                base.Write(buffer, index, count);
                if (AutoFlush) Flush();
            }
        }

        /// <summary>
        /// OBSOLETE: Use DirectoryCopyRecursive for simple recursive copy 
        /// or AdvDirectoryCopy for whitelist/blacklist functionality.
        /// By default, copies directory with all subdirectories recursively.
        /// </summary>
        /// <param name="sourceDirName">full path</param>
        /// <param name="destDirName">full path</param>
        /// <param name="copySubDirs">if false, will copy only top dir</param>
        /// <param name="excludeList">list of directory names (just dir names) not to copy, case insensitive</param>
        [Obsolete]
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true, string[] excludeList = null)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            if (!Directory.Exists(destDirName))
            {
                Debug.WriteLine("create dir " + destDirName);
                Directory.CreateDirectory(destDirName);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                Debug.WriteLine("copying " + file.FullName);
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            if (copySubDirs)
            {
                if (excludeList != null)
                {
                    excludeList = excludeList.Select(x => x.ToLower()).ToArray();
                }

                foreach (DirectoryInfo subdir in dirs)
                {
                    if (excludeList == null || !excludeList.Contains(subdir.Name.ToLower()))
                    {
                        string temppath = Path.Combine(destDirName, subdir.Name);
                        DirectoryCopy(subdir.FullName, temppath, copySubDirs, excludeList);
                    }
                }
            }
        }

        /// <summary>
        /// A copier that can be used to copy directory, with subdirectories and optionally using 
        /// whitelist or blacklist dir paths. White and blacklists ignore case, this may not be suitable for Linux!
        /// </summary>
        /// <remarks>
        /// This class is just a helper for actual public static method.
        /// </remarks>
        class DirectoryCopier
        {
            HashSet<string> PathWhitelist = null;
            HashSet<string> PathBlacklist = null;

            /// <summary>
            /// Copies directory, by default includes all subdirectories recursively. 
            /// Can optionally use a whitelist or blacklist dir paths.
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
            /// <param name="sourceDirName">full path</param>
            /// <param name="destDirName">full path</param>
            /// <param name="copySubDirs">true to copy all subdirs, false to copy only top dir</param>
            /// <param name="whitelist">list of full subdir paths to copy recursively, rest is ignored</param>
            /// <param name="blacklist">list of full subdir paths to not copy recursively, rest is copied</param>
            /// <exception cref="InvalidOperationException">whitelist and blacklist can't be used simultaneously</exception>
            public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true, string[] whitelist = null, string[] blacklist = null)
            {
                if (whitelist != null && blacklist != null) throw new InvalidOperationException("Can't use both whitelist and blacklist");

                PathWhitelist = null;
                PathBlacklist = null;

                try
                {
                    if (whitelist != null) PathWhitelist = BuildSet(whitelist);
                    else if (blacklist != null) PathBlacklist = BuildSet(blacklist);

                    DirectoryCopyAlgorithm(sourceDirName, destDirName, copySubDirs);
                }
                finally
                {
                    //cleanup
                    PathWhitelist = null;
                    PathBlacklist = null;
                }
            }

            HashSet<string> BuildSet(string[] sourceList)
            {
                var result = new HashSet<string>();
                foreach (var path in sourceList)
                {
                    result.Add(path.ToUpperInvariant());
                }
                return result;
            }

            void DirectoryCopyAlgorithm(string sourceDirName, string destDirName, bool copySubDirs = true, bool whitelistOverride = false, bool whitelistContinuation = false)
            {
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);
                DirectoryInfo[] dirs = dir.GetDirectories();

                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + sourceDirName);
                }

                //directory structure should be created only when actual whitelisted subdir needs copying,
                //else should be ignored to avoid clutter
                if (!whitelistContinuation && !Directory.Exists(destDirName))
                {
                    Debug.WriteLine("create dir " + destDirName);
                    Directory.CreateDirectory(destDirName);
                }

                //files should not be copied when algorithm scans for lower-level whitelisted subdirs
                if (!whitelistContinuation)
                {
                    FileInfo[] files = dir.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        Debug.WriteLine("copying " + file.FullName);
                        string temppath = Path.Combine(destDirName, file.Name);
                        file.CopyTo(temppath, false);
                    }
                }

                if (copySubDirs)
                {
                    if (PathWhitelist != null)
                    {
                        foreach (DirectoryInfo subdir in dirs)
                        {
                            //if path is whitelisted, any subpaths and subfiles are whitelisted as well
                            if (whitelistOverride || PathWhitelist.Contains(subdir.FullName.ToUpperInvariant()))
                            {
                                string temppath = Path.Combine(destDirName, subdir.Name);
                                DirectoryCopyAlgorithm(subdir.FullName, temppath, copySubDirs, whitelistOverride: true);
                            }
                            //even if its not on whitelist, it's possible subdir is on whitelist
                            else
                            {
                                string temppath = Path.Combine(destDirName, subdir.Name);
                                DirectoryCopyAlgorithm(subdir.FullName, temppath, copySubDirs, whitelistContinuation: true);
                            }
                        }
                    }
                    else if (PathBlacklist != null)
                    {
                        foreach (DirectoryInfo subdir in dirs)
                        {
                            if (!PathBlacklist.Contains(subdir.FullName.ToUpperInvariant()))
                            {
                                string temppath = Path.Combine(destDirName, subdir.Name);
                                DirectoryCopyAlgorithm(subdir.FullName, temppath, copySubDirs);
                            }
                        }
                    }
                    else
                    {
                        foreach (DirectoryInfo subdir in dirs)
                        {
                            string temppath = Path.Combine(destDirName, subdir.Name);
                            DirectoryCopyAlgorithm(subdir.FullName, temppath, copySubDirs);
                        }
                    }
                }
            }
        }

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
        public static void AdvDirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true, string[] whitelist = null, string[] blacklist = null)
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
            AdvDirectoryCopy(sourceDirName, destDirName);
        }
    }
}
