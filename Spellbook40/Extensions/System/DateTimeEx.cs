using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Aldurcraft.Spellbook40.WizardTower;

namespace Aldurcraft.Spellbook40.Extensions.System
{
    public static class DateTimeEx
    {
        public static string ToStringFormattedForFileEx(this DateTime dateTime, bool highPrecision = false)
        {
            if (highPrecision)
            {
                return dateTime.ToString("yyyy-MM-dd__HH-mm-ss_FFFFFF");
            }
            else
            {
                return dateTime.ToString("yyyy-MM-dd__HH-mm-ss");
            }
        }

        /// <summary>
        /// returns false if failed to parse or exc, dt will be minValue
        /// </summary>
        /// <param name="input"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static bool TryGetDateTimeFromNowString(string input, out DateTime dt)
        {
            try
            {
                Match match = Regex.Match(input, @"(\d\d\d\d)-(\d\d)-(\d\d)__(\d\d)-(\d\d)-(\d\d)");
                if (!match.Success)
                {
                    dt = DateTime.MinValue;
                    return false;
                }
                else
                {
                    dt = new DateTime(
                        Convert.ToInt32(match.Groups[1].Value),
                        Convert.ToInt32(match.Groups[2].Value),
                        Convert.ToInt32(match.Groups[3].Value),
                        Convert.ToInt32(match.Groups[4].Value),
                        Convert.ToInt32(match.Groups[5].Value),
                        Convert.ToInt32(match.Groups[6].Value));
                    return true;
                }
            }
            catch (Exception _e)
            {
                SpellbookLogger.LogError("TimeHelper TryGetDateTimeFromNowString error " + _e.Message, "DateTimeEx", _e);
                dt = DateTime.MinValue;
                return false;
            }
        }
    }
}
