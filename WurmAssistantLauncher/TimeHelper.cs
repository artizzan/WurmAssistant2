using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WurmAssistantLauncher
{
    public static class TimeHelper
    {
        public static string GetStringForDateNow()
        {
            return DateTime.Now.ToString("yyyy-MM-dd__HH-mm-ss");
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
                System.Diagnostics.Debug.WriteLine("TimeHelper TryGetDateTimeFromNowString error "+_e.Message);
                //Aldurcraft.LoggingEngine.Logger.LogDebug("", "TimeHelpers", _e);
                dt = DateTime.MinValue;
                return false;
            }
        }

        /// <summary>
        /// Converts timespan to something like: "9d", "3d 22h", "18h", "3h 22m", "22m"
        /// </summary>
        /// <param name="timespan"></param>
        /// <returns></returns>
        /// <remarks>
        /// Shows just minutes for t less than 1h, only hours between 6-24h, only days if 6d or more
        /// </remarks>
        public static string FormatTimeSpanForDisplay(TimeSpan timespan)
        {
            double totalHours = timespan.TotalHours;
            if (totalHours < 0)
            {
                return "error";
            }
            else if (totalHours < 1)
            {
                return timespan.ToString("m'm'");
            }
            else if (totalHours < 6)
            {
                return timespan.ToString("h'h 'm'm'");
            }
            else if (totalHours < 24)
            {
                return timespan.ToString("h'h'");
            }
            else if (totalHours < 144)
            {
                return timespan.ToString("d'd 'h'h'");
            }
            else
            {
                return timespan.ToString("d'd'");
            }
        }
    }
}
