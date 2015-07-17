using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Aldurcraft.Spellbook40.Extensions.System.IO
{
    public static class IoEx
    {
        static char DirSeparator = Path.DirectorySeparatorChar;

        /// <summary>
        /// Returns name of the last directory in this absolute directory path. 
        ///  Note: this method is intended for directory paths, behaves differently for file paths;
        ///  for dir path "drive:\previousDir\lastDir\", it will return "lastDir"; 
        ///  in case of file path "drive:\previousDir\lastDir\filename.ext, it will return "filename.ext"
        /// </summary>
        /// <param name="path">valid directory path</param>
        /// <returns></returns>
        public static string GetLastDirNamefromDirPath(string path)
        {
            try
            {
                path.Trim();
                if (path.LastIndexOf(DirSeparator) == path.Length - 1) path = path.Remove(path.Length - 1, 1);
                return path = path.Remove(0, (path.LastIndexOf(DirSeparator) + 1));
            }
            catch
            {
                if (path != null) return path;
                else return null;
            }
        }

        /// <summary>
        /// Returns name of the previous to last directory (1 level down) in this absolute directory path. 
        ///  Note: this method is intended for directory paths, behaves differently for file paths;
        ///  for dir path "drive:\previousDir\lastDir\", it will return "previousDir"; 
        ///  in case of file path "drive:\previousDir\lastDir\filename.ext, it will return "lastDir"
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPreviousDirNameFromDirPath(string path)
        {
            try
            {
                path = path.Trim();
                // remove last "\" if exists
                if (path.LastIndexOf(DirSeparator) == path.Length - 1) path = path.Remove(path.Length - 1, 1);
                // strip path of bottom dir including leading "\"
                path = path.Substring(0, path.LastIndexOf(DirSeparator));
                // strip path of everything except last dir name
                return path = path.Remove(0, (path.LastIndexOf(DirSeparator) + 1));
            }
            catch
            {
                if (path != null) return path;
                else return null;
            }
        }

        /// <summary>
        /// returns complete path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPathToDirectoryAbove(string path)
        {
            try
            {
                path = path.Trim();
                // remove last "\" if exists
                if (path.LastIndexOf(DirSeparator) == path.Length - 1) path = path.Remove(path.Length - 1, 1);
                // strip path of bottom dir including leading "\"
                return path = path.Substring(0, path.LastIndexOf(DirSeparator));
            }
            catch
            {
                if (path != null) return path;
                else return null;
            }
        }
        static string CachedCodeBasePath = null;
        /// <summary>
        /// Uses Path.Combine() to merge provided path with executing assembly base directory path.
        /// If provided path is null or empty, will return base directory path.
        /// </summary>
        /// <param name="path">path relative to the executing assembly base directory</param>
        /// <returns></returns>
        public static string PathCombineWithCodeBasePath(string path)
        {
            string pth;
            if (CachedCodeBasePath != null) pth = CachedCodeBasePath;
            else
            {
                pth = (new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath;
                pth = Path.GetDirectoryName(pth);
                //pth = pth.Replace("%20", " ");
                CachedCodeBasePath = pth;
            }
            if (!String.IsNullOrEmpty(path))
                pth = Path.Combine(pth, path);
            return pth;
        }
    }
}
