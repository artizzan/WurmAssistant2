using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Drawing;
using Aldurcraft.Utility.WurmHelpers;

namespace Aldurcraft.Utility
{
    /// <summary>
    /// General unsorted utilities, some of these methods may be rewired into specific helpers at later dates.
    /// </summary>
    public static class GeneralHelper
    {
        #region Path and Directory Helpers

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
                pth = (new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath;
                pth = Path.GetDirectoryName(pth);
                //pth = pth.Replace("%20", " ");
                CachedCodeBasePath = pth;
            }
            if (!String.IsNullOrEmpty(path))
                pth = Path.Combine(pth, path);
            return pth;
        }

        #endregion

        #region String Parsing

        /// <summary>
        /// Is the supplied object a number (int16,32,64,decimal,single,double,boolean)
        /// </summary>
        /// <param name="Expression"></param>
        /// <returns></returns>
        public static bool IsNumeric(Object Expression)
        {
            if (Expression == null || Expression is DateTime)
                return false;

            if (Expression is Int16 || Expression is Int32 || Expression is Int64 || Expression is Decimal || Expression is Single || Expression is Double || Expression is Boolean)
                return true;

            try
            {
                if (Expression is string)
                    Double.Parse(Expression as string);
                else
                    Double.Parse(Expression.ToString());
                return true;
            }
            catch { } // just dismiss errors but return false
            return false;
        }

        /// <summary>
        /// REGEX: Attempts to cnvert a Match to Int32. Returns 0 on failure.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int MatchToInt32(Match match)
        {
            try
            {
                return Convert.ToInt32(Regex.Match(match.ToString(), @"\d\d*").ToString());
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// wurm-online-specific code, moved to WurmHelper
        /// returns -1 if failed
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        [Obsolete]
        public static float ExtractSkillGAINFromLine(string line)
        {
            return WurmHelper.ExtractSkillGAINFromLine(line);
        }

        /// <summary>
        /// wurm-online-specific code, moved to WurmHelper
        /// returns -1 if failed
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        [Obsolete] //wurm-online-specific code, moved to WurmHelper
        public static float ExtractSkillLEVELFromLine(string line)
        {
            return WurmHelper.ExtractSkillLEVELFromLine(line);
        }

        #endregion

        /// <summary>
        /// If the value is less than min or more than max, returns min or max respectively, else returns the value.
        /// Intended for aligning value to min/max constraints, NOT for checking of it's valid (naming fail ^_^)
        /// </summary>
        /// <typeparam name="T">Any comparable type</typeparam>
        public static T ConstrainValue<T>(T value, T min, T max) where T : System.IComparable<T>
        {
            if (value.CompareTo(min) < 0) return min;
            else if (value.CompareTo(max) > 0) return max;
            else return value;
        }


    }
}
