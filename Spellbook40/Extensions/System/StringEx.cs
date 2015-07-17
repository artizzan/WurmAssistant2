using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Aldurcraft.Spellbook40.Extensions.System
{
    public static class StringEx
    {
        public static bool ContainsEx(this string source, string toCheck, StringComparison comp)
        {
            if (toCheck == null) return false;
            return source.IndexOf(toCheck, comp) >= 0;
        }

        /// <summary>
        /// Will wrap the input text if it's too long. Intended for setting tooltip text.
        /// </summary>
        /// <param name="source">string source</param>
        /// <param name="maxCharsPerLine">maximum number of characters per line</param>
        /// <returns></returns>
        public static string GetLineWrappedStringEx(this string source, int maxCharsPerLine = 40)
        {
            return Regex.Replace(source, @"(.{" + maxCharsPerLine + @"}\s)", "$1\r\n");
        }

        public static string CapitalizeEx(this string source)
        {
            for (int i = 0; i < source.Length; i++)
            {
                var x = source[i];
                if (!char.IsWhiteSpace(x))
                {
                    if (char.IsLetter(x))
                    {
                        var capitalized = char.ToUpper(x, CultureInfo.CurrentCulture);
                        return source.Substring(0, i) + capitalized + source.Substring(i + 1);
                    }
                }
            }
            return source;
        }
    }
}
