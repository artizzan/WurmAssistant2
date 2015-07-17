using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmLogsManager
{
    /// <summary>
    /// All wurm log types. All PM logs have the same type
    /// </summary>
    public enum GameLogTypes
    {
        Combat, Event, Friends, Local, Skills, Alliance,
        CA_HELP, Freedom, GLFreedom, MGMT, PM, Team, Village,
        Deaths,
        MolRehan,
        JennKellon,
        GLMolRehan,
        GLJennKellon,
        HOTS,
        GLHOTS
    };

    /// <summary>
    /// Provides mapping for string name to enum and reverse for easy lookups
    /// </summary>
    public static class GameLogTypesEX
    {
        static Dictionary<string, GameLogTypes> NameToEnumMap = new Dictionary<string, GameLogTypes>();
        static Dictionary<GameLogTypes, string> EnumToNameMap = new Dictionary<GameLogTypes, string>();

        static GameLogTypesEX()
        {
            AddLogTypeMapping("_Combat", GameLogTypes.Combat);
            AddLogTypeMapping("_Event", GameLogTypes.Event);
            AddLogTypeMapping("_Friends", GameLogTypes.Friends);
            AddLogTypeMapping("_Local", GameLogTypes.Local);
            AddLogTypeMapping("_Skills", GameLogTypes.Skills);
            AddLogTypeMapping("Alliance", GameLogTypes.Alliance);
            AddLogTypeMapping("CA_HELP", GameLogTypes.CA_HELP);
            AddLogTypeMapping("Freedom", GameLogTypes.Freedom);
            AddLogTypeMapping("GL-Freedom", GameLogTypes.GLFreedom);
            AddLogTypeMapping("MGMT", GameLogTypes.MGMT);
            AddLogTypeMapping("PM", GameLogTypes.PM);
            AddLogTypeMapping("Team", GameLogTypes.Team);
            AddLogTypeMapping("Village", GameLogTypes.Village);
            AddLogTypeMapping("_Deaths", GameLogTypes.Deaths);
            AddLogTypeMapping("Mol_Rehan", GameLogTypes.MolRehan);
            AddLogTypeMapping("GL-Mol_Rehan", GameLogTypes.GLMolRehan);
            AddLogTypeMapping("Jenn-Kellon", GameLogTypes.JennKellon);
            AddLogTypeMapping("GL-Jenn-Kellon", GameLogTypes.GLJennKellon);
            AddLogTypeMapping("HOTS", GameLogTypes.HOTS);
            AddLogTypeMapping("GL-HOTS", GameLogTypes.GLHOTS);
        }

        static void AddLogTypeMapping(string logName, GameLogTypes logType)
        {
            NameToEnumMap.Add(logName, logType);
            EnumToNameMap.Add(logType, logName);
        }

        public static bool DoesTypeExist(string logName)
        {
            return NameToEnumMap.ContainsKey(logName);
        }

        public static string GetNameForLogType(GameLogTypes type)
        {
            return EnumToNameMap[type];
        }

        public static GameLogTypes GetLogTypeForName(string logName)
        {
            return NameToEnumMap[logName];
        }

        public static string[] GetAllNames()
        {
            return NameToEnumMap.Keys.ToArray();
        }

        public static GameLogTypes[] GetAllLogTypes()
        {
            return EnumToNameMap.Keys.ToArray();
        }
    }

    /// <summary>
    /// Wrapper around single Wurm Online log file. 
    /// Auto aquires file when possible and auto reaquires proper file at midnight change.
    /// </summary>
    internal class GameLogState
    {
        // path to log file
        string logAddress;
        // path to log directory
        string logDirPath;
        // type of this log
        GameLogTypes _logType;
        internal GameLogTypes LogType
        {
            get { return _logType; }
        }
        // type of logging mode
        bool DailyLoggingMode;
        // text file wrapper associated with this log wrapper
        TextFileObject LogFile;
        // list of all new lines in log, compared to previous snapshot
        List<string> newLinesInLog = new List<string>();
        // used to avoid parsing lines already inside log file at engine initialization
        bool isInitialized = false;
        // displays all events in program log as they appear in log
        internal bool displayEvents = false;
        // time when this log was aquired, used to track midnight change
        DateTime logAquiredOnDate;
        // name of the PM sender, used to build proper file path for this wrapper
        string pm_name = "";
        internal string PM_Name
        {
            get { return pm_name; }
        }
        // holds log file name string
        string LogFileName;

        // true if underlying text file was accessible on last update try
        internal bool LogTextFileExists
        {
            get { return LogFile.FileExists; }
        }

        string LogPatternToSearchFor;

        /// <summary>
        /// constructs new log wrapper, name required ONLY for PM logs
        /// </summary>
        /// <param name="wurmLogDirAddress">path to the Wurm Online Logs folder for chosen player account</param>
        /// <param name="logtype">type of this log, determines what files will be searched for aquiring</param>
        /// <param name="name">name of the PM player, if this is PM log wrapper</param>
        internal GameLogState(string wurmLogDirAddress, GameLogTypes logtype, bool dailyLoggingMode, string name = "")
        {
            this._logType = logtype;
            this.DailyLoggingMode = dailyLoggingMode;
            if (this._logType == GameLogTypes.PM)
            {
                this.pm_name = "__" + name;
            }
            this.logDirPath = wurmLogDirAddress;
            InitLogState();
        }

        void InitLogState()
        {
            isInitialized = false;
            SetPathToLogFile(this.logDirPath);
            this.LogFile = new TextFileObject(this.logAddress, true, false, false, true, true, true, true);
            this.UpdateAndGetNewEvents();
        }

        void SetPathToLogFile(string wurmLogDirAddress)
        {
            //get current date
            DateTime DateNow = DateTime.Now;
            logAquiredOnDate = DateNow;

            //note: because pm logs handling was added to wurmassistantengine, on top of original design,
            //      any changes here should be reflected there and vice versa
            string logDateFormat;
            if (DailyLoggingMode) logDateFormat = DateNow.ToString("yyyy-MM-dd");
            else logDateFormat = DateNow.ToString("yyyy-MM");

            LogPatternToSearchFor = GetLogStringForType() + pm_name + "." + logDateFormat + ".txt";
            System.Diagnostics.Debug.WriteLine(LogPatternToSearchFor);
            LogFileName = LogPatternToSearchFor;
            logAddress = wurmLogDirAddress + @"\" + LogPatternToSearchFor;
            System.Diagnostics.Debug.WriteLine(logAddress);
        }

        internal string debugShowLogPattern()
        {
            return LogPatternToSearchFor;
        }

        /// <summary>
        /// Returns this log type
        /// </summary>
        /// <returns>Enum GameLogTypes</returns>
        internal GameLogTypes GetLogType()
        {
            return _logType;
        }

        internal string GetLogName()
        {
            return LogFileName;
        }

        string GetLogStringForType()
        {
            System.Diagnostics.Debug.WriteLine(_logType);
            return GameLogTypesEX.GetNameForLogType(_logType);
        }

        /// <summary>
        /// Updates the snapshot of log file contents, retrieves and returns list of new lines in the log since previous update
        /// </summary>
        /// <returns>null if no new lines</returns>
        internal List<string> UpdateAndGetNewEvents()
        {
            handleDayChange();
            LogFile.Update();

            if (LogFile.FileExists)
            {
                string nextLogLine;
                newLinesInLog.Clear();
                bool newLogData = false;

                while ((nextLogLine = LogFile.ReadNextLineOffset(0)) != null)
                {
                    if (nextLogLine != null)
                    {
                        newLinesInLog.Add(nextLogLine);
                        newLogData = true;
                    }
                }

                if (!isInitialized)
                {
                    isInitialized = true;
                    return null;
                }

                else if (newLogData == true)
                {
                    if (displayEvents)
                    {
                        foreach (string line in newLinesInLog)
                        {
                            Logger.LogInfo(GetLogStringForType() + ": line");
                        }
                    }
                    return newLinesInLog;
                }
                else return null;
            }
            else return null;
        }

        void handleDayChange()
        {
            if (DateTime.Now.Day != logAquiredOnDate.Day)
            {
                InitLogState();
            }
        }
    }
}
