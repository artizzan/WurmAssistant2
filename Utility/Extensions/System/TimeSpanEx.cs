using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aldurcraft.Utility;

namespace System.Ex
{
    public static class TimeSpanEx
    {
        /// <summary>
        /// Multiplies a timespan by an integer value
        /// </summary>
        public static TimeSpan Multiply(this TimeSpan multiplicand, int multiplier)
        {
            return TimeSpan.FromTicks(multiplicand.Ticks * multiplier);
        }

        /// <summary>
        /// Multiplies a timespan by a double value
        /// </summary>
        public static TimeSpan Multiply(this TimeSpan multiplicand, double multiplier)
        {
            return TimeSpan.FromTicks((long)(multiplicand.Ticks * multiplier));
        }

        public static string ToFriendlyStringEx(this TimeSpan ts)
        {
            return TimeHelper.FormatTimeSpanForDisplay(ts);
        }

        /// <summary>
        /// Multiplies a timespan by an integer value
        /// </summary>
        public static TimeSpan MultiplyEx(this TimeSpan multiplicand, int multiplier)
        {
            return TimeSpan.FromTicks(multiplicand.Ticks * multiplier);
        }

        /// <summary>
        /// Multiplies a timespan by a double value
        /// </summary>
        public static TimeSpan MultiplyEx(this TimeSpan multiplicand, double multiplier)
        {
            return TimeSpan.FromTicks((long)(multiplicand.Ticks * multiplier));
        }

        /// <summary>
        /// Converts timespan to something like: "9d", "3d 22h", "18h", "3h 22m", "22m"
        /// </summary>
        /// <param name="timespan"></param>
        /// <returns></returns>
        /// <remarks>
        /// Shows just minutes for t less than 1h, only hours between 6-24h, only days if 6d or more
        /// </remarks>
        public static string FormatForConciseDisplayEx(this TimeSpan timespan)
        {
            double totalMinutes = timespan.TotalMinutes;
            if (totalMinutes < 0)
            {
                return "error";
            }
            else if (totalMinutes < 10)
            {
                return timespan.ToString("m'm 's's'");
            }
            else if (totalMinutes < 1 * 60)
            {
                return timespan.ToString("m'm'");
            }
            else if (totalMinutes < 6 * 60)
            {
                return timespan.ToString("h'h 'm'm'");
            }
            else if (totalMinutes < 24 * 60)
            {
                return timespan.ToString("h'h'");
            }
            else if (totalMinutes < 144 * 60)
            {
                return timespan.ToString("d'd 'h'h'");
            }
            else
            {
                return timespan.ToString("d'd'");
            }
        }

        public static string FormatConciseToMinutesEx(this TimeSpan timespan)
        {
            double totalMinutes = timespan.TotalMinutes;
            if (totalMinutes < 0)
            {
                return "error";
            }
            else if (totalMinutes < 1 * 60)
            {
                return timespan.ToString("m'm'");
            }
            else if (totalMinutes < 6 * 60)
            {
                return timespan.ToString("h'h 'm'm'");
            }
            else if (totalMinutes < 24 * 60)
            {
                return timespan.ToString("h'h'");
            }
            else if (totalMinutes < 144 * 60)
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
