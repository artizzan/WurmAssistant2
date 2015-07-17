using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmState
{
    public enum WurmStarfall { Diamond = 1, Saw, Digging, Leaf, Bear, Snake, WhiteShark, Fire, Raven, Dancer, Omen, Silent }
    public enum WurmDay { Ant = 1, Luck, Wurm, Wrath, Tears, Sleep, Awakening }

    public class NoSuchWurmDayException : Exception
    {
    }

    public class NoSuchWurmStarfallException : Exception
    {
    }

    /// <summary>
    /// Utility class for working with WurmDay
    /// </summary>
    public static class WurmDayEX
    {
        const string
            ANT = "Ant",
            LUCK = "Luck",
            WURM = "Wurm",
            WRATH = "Wrath",
            TEARS = "Tears",
            SLEEP = "Sleep",
            AWAKENING = "Awakening";

        public static readonly string[] WurmDaysNames = { ANT, LUCK, WURM, WRATH, TEARS, SLEEP, AWAKENING };

        /// <summary>
        /// Returns wurm name of the enum-representation of wurm day. Exception if enum not recognized.
        /// </summary>
        /// <param name="dayEnum"></param>
        /// <returns></returns>
        /// <exception cref="NoSuchWurmDayException">Enum was not valid</exception>
        public static string GetNameForEnum(WurmDay dayEnum)
        {
            switch (dayEnum)
            {
                case WurmDay.Ant:
                    return ANT;
                case WurmDay.Awakening:
                    return AWAKENING;
                case WurmDay.Luck:
                    return LUCK;
                case WurmDay.Sleep:
                    return SLEEP;
                case WurmDay.Tears:
                    return TEARS;
                case WurmDay.Wrath:
                    return WRATH;
                case WurmDay.Wurm:
                    return WURM;
                default:
                    throw new NoSuchWurmDayException();
            }
        }

        /// <summary>
        /// Returns enum-representation of wurm day from it's wurm name. Exception if name not recognized.
        /// </summary>
        /// <param name="name">case sensitive</param>
        /// <returns></returns>
        /// <exception cref="NoSuchWurmDayException">string representing name was not valid</exception>
        public static WurmDay GetEnumForName(string name)
        {
            switch (name)
            {
                case ANT:
                    return WurmDay.Ant;
                case AWAKENING:
                    return WurmDay.Awakening;
                case LUCK:
                    return WurmDay.Luck;
                case SLEEP:
                    return WurmDay.Sleep;
                case TEARS:
                    return WurmDay.Tears;
                case WRATH:
                    return WurmDay.Wrath;
                case WURM:
                    return WurmDay.Wurm;
                default:
                    throw new NoSuchWurmDayException();
            }
        }
    }

    /// <summary>
    /// Utility class for working with WurmStarfall
    /// </summary>
    public static class WurmStarfallEX
    {
        const string
            DIAMOND = "Diamond",
            SAW = "Saw",
            DIGGING = "Digging",
            LEAF = "Leaf",
            BEAR = "Bear",
            SNAKE = "Snake",
            WHITESHARK = "White Shark",
            FIRE = "Fire",
            RAVEN = "Raven",
            DANCER = "Dancer",
            OMEN = "Omen",
            SILENT = "Silence";

        public static readonly string[] WurmStarfallNames = { DIAMOND, SAW, DIGGING, LEAF, BEAR, SNAKE, WHITESHARK, FIRE, RAVEN, DANCER, OMEN, SILENT };

        /// <summary>
        /// Returns wurm name of the enum-representation of wurm starfall. Exception if enum not recognized.
        /// </summary>
        /// <param name="dayEnum"></param>
        /// <returns></returns>
        /// <exception cref="NoSuchWurmStarfallException">Enum was not valid</exception>
        public static string GetNameForEnum(WurmStarfall dayEnum)
        {
            switch (dayEnum)
            {
                case WurmStarfall.Bear:
                    return BEAR;
                case WurmStarfall.Dancer:
                    return DANCER;
                case WurmStarfall.Diamond:
                    return DIAMOND;
                case WurmStarfall.Digging:
                    return DIGGING;
                case WurmStarfall.Fire:
                    return FIRE;
                case WurmStarfall.Leaf:
                    return LEAF;
                case WurmStarfall.Omen:
                    return OMEN;
                case WurmStarfall.Raven:
                    return RAVEN;
                case WurmStarfall.Saw:
                    return SAW;
                case WurmStarfall.Silent:
                    return SILENT;
                case WurmStarfall.Snake:
                    return SNAKE;
                case WurmStarfall.WhiteShark:
                    return WHITESHARK;
                default:
                    throw new NoSuchWurmStarfallException();
            }
        }

        /// <summary>
        /// Returns enum-representation of wurm starfall from it's wurm name. Exception if name not recognized.
        /// </summary>
        /// <param name="name">case sensitive</param>
        /// <returns></returns>
        /// <exception cref="NoSuchWurmStarfallException">string representing name was not valid</exception>
        public static WurmStarfall GetEnumForName(string name)
        {
            switch (name)
            {
                case BEAR:
                    return WurmStarfall.Bear;
                case DANCER:
                    return WurmStarfall.Dancer;
                case DIAMOND:
                    return WurmStarfall.Diamond;
                case DIGGING:
                    return WurmStarfall.Digging;
                case FIRE:
                    return WurmStarfall.Fire;
                case LEAF:
                    return WurmStarfall.Leaf;
                case OMEN:
                    return WurmStarfall.Omen;
                case RAVEN:
                    return WurmStarfall.Raven;
                case SAW:
                    return WurmStarfall.Saw;
                case SILENT:
                    return WurmStarfall.Silent;
                case SNAKE:
                    return WurmStarfall.Snake;
                case WHITESHARK:
                    return WurmStarfall.WhiteShark;
                default:
                    throw new NoSuchWurmStarfallException();
            }
        }
    }

    /// <summary>
    /// Represents a single date/time in Wurm calendar. Usage mimicks (some) functionality of System.DateTime
    /// </summary>
    /// <remarks>
    /// WurmDateTime specification:
    /// 24h day time, there are 7 days in each of 4 weeks of every starfall,
    /// there are 12 starfalls in each year. Years start at 0 and max at 99999.
    /// Resulution is to a single second.
    /// Day, Week and Starfall start at 1 (including casting enums to values)
    /// Regular .NET TimeSpan can be added to and subtracted from this date (TimeSpan interpreted as wurm total days/time).
    /// Supports comparison, equality, hashing and serialization.
    /// Built for efficient access to members, not storage efficiency (each public field has cached value).
    /// Note: day value represents day in particular WEEK not starfall, 
    /// to keep the representation in sync with how game client shows the dates.
    /// Overrides == != <![CDATA[<]]> <![CDATA[<=]]> <![CDATA[>]]> <![CDATA[>=]]>
    /// </remarks>
    [Serializable]
    public struct WurmDateTime : IComparable<WurmDateTime>
    {
        /// <summary>
        /// wurm time (val) / realtime (1)
        /// </summary>
        public const double WurmTimeToRealTimeRatio = 8.0D;

        private readonly long totalseconds;

        public long TotalSeconds
        {
            get { return this.totalseconds; }
        }

        private int second;

        public int Second
        {
            get { return this.second; }
        }

        private int minute;

        public int Minute
        {
            get { return this.minute; }
        }

        private int hour;

        public int Hour
        {
            get { return this.hour; }
        }

        private int day;

        private WurmDay dayEnum;

        public WurmDay Day
        {
            get { return this.dayEnum; }
        }

        private int week;

        public int Week
        {
            get { return this.week; }
        }

        private int starfall;

        private WurmStarfall starfallEnum;

        public WurmStarfall Starfall
        {
            get { return this.starfallEnum; }
        }

        private int year;

        public int Year
        {
            get { return this.year; }
        }

        private const int minuteSecs = 60,
                          hourSecs = 60 * 60,
                          daySecs = 24 * 60 * 60,
                          weekSecs = 7 * 24 * 60 * 60,
                          starfallSecs = 4 * 7 * 24 * 60 * 60,
                          yearSecs = 12 * 4 * 7 * 24 * 60 * 60;

        public static WurmDateTime MinValue
        {
            get { return new WurmDateTime(0); }
        }

        public static WurmDateTime MaxValue
        {
            get { return new WurmDateTime(99999, 12, 4, 7, 23, 59, 59); }
        }

        public int DayInYear
        {
            get { return this.day + (this.week - 1) * 7 + (this.starfall - 1) * 28; }
        }

        public TimeSpan DayAndTimeOfYear
        {
            get { return new TimeSpan(this.DayInYear, this.hour, this.minute, this.second); }
        }

        /// <summary>
        /// Creates a new Wurm date/time object
        /// </summary>
        /// <param name="year">0 to 99999</param>
        /// <param name="starfall">1 to 12</param>
        /// <param name="week">1 to 4</param>
        /// <param name="day">1 to 7</param>
        /// <param name="hour">0 to 23</param>
        /// <param name="minute">0 to 59</param>
        /// <param name="second">0 to 59</param>
        public WurmDateTime(int year, int starfall, int week, int day, int hour, int minute, int second)
        {
            ValidateParameter(0, 99999, year, "year");
            ValidateParameter(1, 12, starfall, "starfall");
            ValidateParameter(1, 4, week, "week");
            ValidateParameter(1, 7, day, "day");
            ValidateParameter(0, 23, hour, "hour");
            ValidateParameter(0, 59, minute, "minute");
            ValidateParameter(0, 59, second, "second");

            this.second = second;
            this.minute = minute;
            this.hour = hour;
            this.day = day;
            this.dayEnum = (WurmDay)day;
            this.week = week;
            this.starfall = starfall;
            this.starfallEnum = (WurmStarfall)starfall;
            this.year = year;

            starfall -= 1;
            week -= 1;
            day -= 1;

            this.totalseconds = second;
            this.totalseconds += minute * minuteSecs;
            this.totalseconds += hour * hourSecs;
            this.totalseconds += day * daySecs;
            this.totalseconds += week * weekSecs;
            this.totalseconds += starfall * starfallSecs;
            this.totalseconds += (long)year * (long)yearSecs;
        }

        private static void ValidateParameter(int minInclusive, int maxInclusive, int value, string sourceName)
        {
            if (value < minInclusive)
            {
                throw new ArgumentException(
                    string.Format(
                        "Invalid WurmDateTime parameter, value {0} is lower than min {1} for argument {2}",
                        value,
                        minInclusive,
                        sourceName));
            }
            if (value > maxInclusive)
            {
                throw new ArgumentException(
                    string.Format(
                        "Invalid WurmDateTime parameter, value {0} is higher than max {1} for argument {2}",
                        value,
                        maxInclusive,
                        sourceName));
            }
        }

        /// <summary>
        /// Create a new Wurm date/time object
        /// </summary>
        /// <param name="year">0 to 99999</param>
        /// <param name="starfall">starfall name</param>
        /// <param name="week">1 to 4</param>
        /// <param name="day">day name</param>
        /// <param name="hour">0 to 23</param>
        /// <param name="minute">0 to 59</param>
        /// <param name="second">0 to 59</param>
        public WurmDateTime(int year, WurmStarfall starfall, int week, WurmDay day, int hour, int minute, int second)
            : this(year, (int)starfall, week, (int)day, hour, minute, second)
        {
        }

        /// <summary>
        /// Attempt to create WurmDateTime from wurm log line, throws exception on error
        /// </summary>
        /// <exception cref="Exception"></exception>
        /// <param name="logLine"></param>
        public static WurmDateTime CreateFromLogLine(string logLine)
        {
            //[16:24:19] It is 09:00:48 on day of the Wurm in week 4 of the Bear's starfall in the year of 1035.

            //time
            Match wurmTime = Regex.Match(logLine, @" \d\d:\d\d:\d\d ", RegexOptions.Compiled);
            if (!wurmTime.Success) throw new Exception("failed to parse time of wurm day from line: "+logLine);
            int hour = Convert.ToInt32(wurmTime.Value.Substring(1, 2));
            int minute = Convert.ToInt32(wurmTime.Value.Substring(4, 2));
            int second = Convert.ToInt32(wurmTime.Value.Substring(7, 2));
            //day
            WurmDay? day = null;
            foreach (string name in WurmDayEX.WurmDaysNames)
            {
                if (Regex.IsMatch(logLine, name, RegexOptions.Compiled))
                {
                    day = WurmDayEX.GetEnumForName(name);
                    break;
                }
            }
            //week
            Match wurmWeek = Regex.Match(logLine, @"week (\d)", RegexOptions.Compiled);
            if (!wurmWeek.Success) throw new Exception("failed to parse wurm week from line: " + logLine);
            int week = Convert.ToInt32(wurmWeek.Groups[1].Value);
            //month(starfall)
            WurmStarfall? starfall = null;
            foreach (string name in WurmStarfallEX.WurmStarfallNames)
            {
                if (Regex.IsMatch(logLine, name))
                {
                    starfall = WurmStarfallEX.GetEnumForName(name);
                    break;
                }
            }
            //year
            Match wurmYear = Regex.Match(logLine, @"in the year of (\d+)", RegexOptions.Compiled);
            if (!wurmYear.Success) throw new Exception("failed to parse year from line: " + logLine);
            int year = Convert.ToInt32(wurmYear.Groups[1].Value);

            if (day == null || starfall == null) throw new Exception("log line was not parsed correctly into day or starfall: " + (logLine ?? "NULL"));

            return new WurmDateTime(year, starfall.Value, week, day.Value, hour, minute, second);
        }

        /// <summary>
        /// Construct Wurm date/time from total wurm seconds value
        /// </summary>
        /// <param name="totalseconds"></param>
        public WurmDateTime(long totalseconds)
        {
            this.totalseconds = totalseconds;

            int year = (int)(totalseconds / (long)yearSecs);
            totalseconds -= (long)year * (long)yearSecs;

            int starfall = (int)(totalseconds / starfallSecs);
            totalseconds -= starfall * starfallSecs;

            int week = (int)(totalseconds / weekSecs);
            totalseconds -= week * weekSecs;

            int day = (int)(totalseconds / daySecs);
            totalseconds -= day * daySecs;

            int hour = (int)(totalseconds / hourSecs);
            totalseconds -= hour * hourSecs;

            int minute = (int)(totalseconds / minuteSecs);
            totalseconds -= minute * minuteSecs;

            int second = (int)(totalseconds);

            this.second = second;
            this.minute = minute;
            this.hour = hour;
            this.day = day + 1;
            this.dayEnum = (WurmDay)(day + 1);
            this.week = week + 1;
            this.starfall = starfall + 1;
            this.starfallEnum = (WurmStarfall)(starfall + 1);
            this.year = year;
        }

        /// <summary>
        /// Returns true if this instance points to time point within supplied min and max constraints.
        /// </summary>
        /// <param name="minDate"></param>
        /// <param name="maxDate"></param>
        /// <returns></returns>
        public bool IsWithin(WurmDateTime minDate, WurmDateTime maxDate)
        {
            return this > minDate && this < maxDate;
        }

        /// <summary>
        /// Returns time difference between this date and supplied date. 
        /// Positive if supplied date is later than this date.
        /// </summary>
        /// <remarks>
        /// Note: breaking change since WA2, reversed polarity (take that borg!).
        /// </remarks>
        /// <param name="otherDate"></param>
        /// <returns></returns>
        public TimeSpan TimeTo(WurmDateTime otherDate)
        {
            return TimeSpan.FromSeconds(otherDate.TotalSeconds - this.TotalSeconds);
        }

        /// <summary>
        /// Returns string representing this date, in a format similar to how Wurm client shows it.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format(
                "{0:00}:{1:00}:{2:00} on day of {3} in week {4} of the {5} starfall in the year of {6}",
                this.hour,
                this.minute,
                this.second,
                this.dayEnum,
                this.week,
                this.starfallEnum,
                this.year);
        }

        public int CompareTo(WurmDateTime other)
        {
            return this.TotalSeconds.CompareTo(other.TotalSeconds);
        }

        public static WurmDateTime operator +(WurmDateTime wdt, TimeSpan ts)
        {
            long val = wdt.TotalSeconds + (long)ts.TotalSeconds;
            if (val > MaxValue.TotalSeconds)
                val = MaxValue.TotalSeconds;
            return new WurmDateTime(val);
        }

        public static WurmDateTime operator -(WurmDateTime wdt, TimeSpan ts)
        {
            long val = wdt.TotalSeconds - (long)ts.TotalSeconds;
            if (val < MinValue.TotalSeconds)
                val = MinValue.TotalSeconds;
            return new WurmDateTime(val);
        }

        public static bool operator >(WurmDateTime arg1, WurmDateTime arg2)
        {
            return arg1.CompareTo(arg2) > 0;
        }

        public static bool operator <(WurmDateTime arg1, WurmDateTime arg2)
        {
            return arg1.CompareTo(arg2) < 0;
        }

        public static bool operator >=(WurmDateTime arg1, WurmDateTime arg2)
        {
            return arg1.CompareTo(arg2) >= 0;
        }

        public static bool operator <=(WurmDateTime arg1, WurmDateTime arg2)
        {
            return arg1.CompareTo(arg2) <= 0;
        }

        public static bool operator ==(WurmDateTime arg1, WurmDateTime arg2)
        {
            return arg1.CompareTo(arg2) == 0;
        }

        public static bool operator !=(WurmDateTime arg1, WurmDateTime arg2)
        {
            return arg1.CompareTo(arg2) != 0;
        }


        /// <summary>
        /// Checks for equality down to a single second resultion
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is WurmDateTime)
            {
                WurmDateTime wdt = (WurmDateTime)obj;
                return this.totalseconds == wdt.totalseconds;
            }
            else
                return false;
        }

        /// <summary>
        /// Checks for equality down to a single second resultion
        /// </summary>
        /// <param name="wdt"></param>
        /// <returns></returns>
        public bool Equals(WurmDateTime wdt)
        {
            return this.totalseconds == wdt.totalseconds;
        }

        public override int GetHashCode()
        {
            return this.totalseconds.GetHashCode();
        }
    }
}
