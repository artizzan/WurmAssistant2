using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Aldurcraft.Utility;
using System.Globalization;

namespace Aldurcraft.Utility.WurmHelpers
{
    /// <summary>
    /// Wurm Online / Wurm Assistant specific helpers.
    /// </summary>
    public static class WurmHelper
    {
        /// <summary>
        /// returns -1 if failed
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static float ExtractSkillGAINFromLine(string line)
        {
            return ExtractNumerFromLine(line, true);
        }

        /// <summary>
        /// returns -1 if failed
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static float ExtractSkillLEVELFromLine(string line)
        {
            return ExtractNumerFromLine(line, false);
        }

        static float ExtractNumerFromLine(string line, bool GAIN)
        {
            string part;
            if (GAIN) part = "by "; else part = "to ";

            float level = -1;
            Match match = Regex.Match(line, part + @"\d+\,\d+");
            if (!match.Success) match = Regex.Match(line, part + @"\d+\.\d+");
            if (!match.Success) match = Regex.Match(line, part + @"\d+");

            if (!match.Success)
            {
                Logger.LogError("! Processed skill line failed to match at ExtractNumerFromLine(" + part + "), line: " + line, "GeneralHelper");
                return -1;
            }
            else if (InvariantParseFloat(match.Value.Substring(3), out level))
            {
                return level;
            }
            else if (InvariantParseFloat(match.Value.Substring(3).Replace(",", "."), out level))
            {
                return level;
            }
            else return -1;
        }

        static bool InvariantParseFloat(string text, out float result)
        {
            return float.TryParse(
                text,
                System.Globalization.NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out result);
        }
    }
}
