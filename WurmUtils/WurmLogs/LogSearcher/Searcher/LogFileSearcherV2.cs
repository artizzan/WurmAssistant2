using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Data;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmLogsManager.Searcher
{
    using DBField = Aldurcraft.Utility.SQLiteDB.DBField;
    /// <summary>
    /// This class is NUTS!
    /// </summary>
    class LogFileSearcherV2
    {
        const string THIS = "LogFileSearcherV2";

        class LogFileData
        {
            const string THIS = "LogFileData";

            static double minutesInDay = new TimeSpan(24, 0, 0).TotalMinutes;

            /// <summary>
            /// Compares two timespans that represent day time in 24-hour format and returns true if 
            ///  they are potentially within 20 minutes, with assumption that timespans can indicate 
            ///  either time on same day or 2 consecutive days in any order.
            /// </summary>
            /// <param name="dt1"></param>
            /// <param name="dt2"></param>
            /// <returns></returns>
            public static bool AreDifferentDayTimesMoreThan20MinAppart(TimeSpan dt1, TimeSpan dt2)
            {
                double TS1val = dt1.TotalMinutes;
                double TS2val = dt2.TotalMinutes;
                double TS2ndval = Math.Abs(TS2val - minutesInDay);

                if (Math.Abs(TS1val - TS2val) > 20D && Math.Abs(TS1val - TS2ndval) > 20D)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Compares two timespans that represent day time in 24-hour format and returns true if 
            ///  they are potentially within one hour, with assumption that timespans can indicate 
            ///  only time on SAME day.
            /// </summary>
            /// <param name="dt1"></param>
            /// <param name="dt2"></param>
            /// <returns></returns>
            public static bool Are_SAME_DayTimesMoreThanAnHourAppart(TimeSpan dt1, TimeSpan dt2)
            {
                double TS1val = dt1.TotalMinutes;
                double TS2val = dt2.TotalMinutes;

                if (Math.Abs(TS1val - TS2val) > 60D)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public static class DBStructure
            {
                public static class CacheTable
                {
                    public static string TableName = "logfilecache_";

                    public static string[] FieldsArray = { "logname PRIMARY KEY NOT NULL", 
                                                        "logdate", 
                                                        "logtype", 
                                                        "playerpm", 
                                                        "filepath", 
                                                        "ismonthly", 
                                                        "lastsize" };

                    public static string LogName = "logname";
                    public static string LogDate = "logdate";
                    public static string LogType = "logtype";
                    public static string PlayerPM = "playerpm";
                    public static string FilePath = "filepath";
                    public static string isMonthly = "ismonthly";
                    public static string LastFileSize = "lastsize";
                }
                public static class FilePositionsTable
                {
                    public static string TableName = "logfilecache_filepositions_";

                    public static string[] FieldsArray = { "logname", 
                                                        "day", 
                                                        "fileposition" };

                    public static string LogName = "logname";
                    public static string Day = "day";
                    public static string FilePosition = "fileposition";
                }
            }

            public string Player;
            public SQLiteDB Database;

            public string LogName;
            public DateTime LogDate;
            public GameLogTypes LogType;
            public string PlayerPM;
            public string FilePath;
            public long LastFileSize;

            public bool isMonthly;
            public Dictionary<int, int> MonthlyFileToDayPositionMap;

            public bool isUnrecognizedFile = false;
            public bool requiresSave = false;

            public LogFileData(string filepath, string player, SQLiteDB database)
            {
                this.Database = database;
                this.Player = player;
                this.LogName = Path.GetFileNameWithoutExtension(filepath);
                // try to load cache from DB
                if (!LoadFromDB())
                {
                    // if not available, parse it from file and mark this object for DB saving
                    ParseAllData(filepath);
                    requiresSave = true;
                }
            }

            public void Update()
            {
                if (isMonthly)
                {
                    // no other update necessary yet
                    UpdatePositionMap();
                    requiresSave = true;
                }
            }

            void ParseAllData(string filepath)
            {
                this.FilePath = filepath;
                // extract name for this log
                this.LogName = Path.GetFileNameWithoutExtension(filepath);
                // extract filename
                string filename = Path.GetFileName(filepath);
                // extract type
                bool fileRecognized = false;
                foreach (string name in GameLogTypesEX.GetAllNames())
                {

                    if (filename.StartsWith(name, StringComparison.Ordinal))
                    {
                        LogType = GameLogTypesEX.GetLogTypeForName(name);
                        fileRecognized = true;
                    }
                }
                if (!fileRecognized) isUnrecognizedFile = true;
                // extract date
                Match matchDate = Regex.Match(filename, @"\d\d\d\d-\d\d-\d\d");
                if (matchDate.Success)
                {
                    string matchstr = matchDate.ToString();
                    LogDate = new DateTime(
                        Convert.ToInt32(matchstr.Substring(0, 4)),
                        Convert.ToInt32(matchstr.Substring(5, 2)),
                        Convert.ToInt32(matchstr.Substring(8, 2)));
                }
                else
                {
                    Match matchDateMonthly = Regex.Match(filename, @"\d\d\d\d-\d\d");
                    if (matchDateMonthly.Success)
                    {
                        string matchstr = matchDateMonthly.ToString();
                        LogDate = new DateTime(
                            Convert.ToInt32(matchstr.Substring(0, 4)),
                            Convert.ToInt32(matchstr.Substring(5, 2)),
                            1);
                        // apply monhtly flag
                        isMonthly = true;
                    }
                    else isUnrecognizedFile = true;
                }
                // extract pm if applies
                if (LogType == GameLogTypes.PM)
                {
                    Match matchPMName = Regex.Match(filename, @"__\w+");
                    string matchPMNamestr = matchPMName.ToString();
                    PlayerPM = matchPMNamestr.Substring(2);
                }

                // parse and note all day positions if monthly file
                if (isMonthly)
                {
                    UpdatePositionMap();
                }

                // note update date
                LastFileSize = new FileInfo(FilePath).Length;
            }

            void UpdatePositionMap()
            {
                MonthlyFileToDayPositionMap = new Dictionary<int, int>();
                try
                {
                    //DebugDump.SetDumpFile(Path.GetFileName(this.FilePath));
                    //DebugDump.ClearFile();

                    TextFileObject textfile = new TextFileObject(this.FilePath, true, true, false, true, true, false, newLineOnlyOnCRLF: true);

                    int lineNumber = 0;
                    string line;
                    TimeSpan lastTimeOfEntry = new TimeSpan(0);
                    int lastDayOfThisFile = 0;
                    while ((line = textfile.ReadNextLine()) != null)
                    {
                        if (line.StartsWith("Logging started", StringComparison.Ordinal))
                        {
                            Match matchThisBlockDate = Regex.Match(line, @"\d\d\d\d-\d\d-\d\d");
                            string strThisBlockDate = matchThisBlockDate.ToString();
                            try
                            {
                                MonthlyFileToDayPositionMap.Add(
                                    Convert.ToInt32(strThisBlockDate.Substring(8, 2)),
                                    lineNumber);
                                lastTimeOfEntry = new TimeSpan(0);
                                lastDayOfThisFile = Convert.ToInt32(strThisBlockDate.Substring(8, 2));
                            }
                            catch
                            {
                                //do nothing, this is another "logging started" on same day, due to relogs 
                            }
                            //DebugDump.WriteLine("loging started " + strThisBlockDate.Substring(8, 2) + " day num: " + lastDayOfThisFile);
                        }
                        else
                        {
                            //DebugDump.WriteLine("regular line begin");
                            try
                            {
                                TimeSpan TimeOfEntry = new TimeSpan(
                                    Convert.ToInt32(line.Substring(1, 2)),
                                    Convert.ToInt32(line.Substring(4, 2)),
                                    Convert.ToInt32(line.Substring(7, 2)));
                                //DebugDump.WriteLine("regular line time: "+TimeOfEntry.ToString());
                                if (TimeOfEntry < lastTimeOfEntry)
                                {
                                    if (Are_SAME_DayTimesMoreThanAnHourAppart(TimeOfEntry, lastTimeOfEntry))
                                    {
                                            lastTimeOfEntry = TimeOfEntry;
                                            lastDayOfThisFile++;
                                            try
                                            {
                                                MonthlyFileToDayPositionMap.Add(
                                                    lastDayOfThisFile,
                                                    lineNumber);
                                            }
                                            catch
                                            {
                                                
                                                Logger.__WriteLine("!LogSearcher: Exception while trying to add new month marker, while updating cache for: " + this.FilePath + ", day num: " + lastDayOfThisFile);
                                                lastDayOfThisFile--;
                                                //DebugDump.WriteLine("exception at regular line dict add");
                                            }
                                            //DebugDump.WriteLine("regular line added to monthly positions, day num: "+ lastDayOfThisFile);
                                    }
                                }
                                lastTimeOfEntry = TimeOfEntry;
                            }
                            catch
                            {
                                //do nothing, its wrong line or smtg
                            }
                        }
                        lineNumber++;
                    }
                }
                catch
                {
                    // this should never happen
                    Debug.WriteLine("some odd error: " + FilePath);
                }
            }

            public void SaveToDB()
            {
                List<DBField> dbfields = new List<DBField>();
                dbfields.Add(new DBField(LogFileData.DBStructure.CacheTable.LogName, this.LogName));
                dbfields.Add(new DBField(LogFileData.DBStructure.CacheTable.LogDate, this.LogDate.ToString(CultureInfo.InvariantCulture)));
                dbfields.Add(new DBField(LogFileData.DBStructure.CacheTable.LogType, GameLogTypesEX.GetNameForLogType(this.LogType)));
                dbfields.Add(new DBField(LogFileData.DBStructure.CacheTable.PlayerPM, this.PlayerPM));
                dbfields.Add(new DBField(LogFileData.DBStructure.CacheTable.FilePath, this.FilePath));
                dbfields.Add(new DBField(LogFileData.DBStructure.CacheTable.isMonthly, this.isMonthly.ToString()));
                dbfields.Add(new DBField(LogFileData.DBStructure.CacheTable.LastFileSize, this.LastFileSize.ToString()));
                if (
                    Database.Update(
                        LogFileData.DBStructure.CacheTable.TableName + Player,
                        dbfields,
                        LogFileData.DBStructure.CacheTable.LogName + "='" + this.LogName + "'"
                    ) == false)
                {
                    Database.Insert(LogFileData.DBStructure.CacheTable.TableName + Player, dbfields);
                }
                if (this.isMonthly)
                {
                    foreach (KeyValuePair<int, int> keyvalue in this.MonthlyFileToDayPositionMap)
                    {
                        List<DBField> dbfields2 = new List<DBField>();
                        dbfields2.Add(new DBField(LogFileData.DBStructure.FilePositionsTable.LogName, this.LogName));
                        dbfields2.Add(new DBField(LogFileData.DBStructure.FilePositionsTable.Day, keyvalue.Key.ToString()));
                        dbfields2.Add(new DBField(LogFileData.DBStructure.FilePositionsTable.FilePosition, keyvalue.Value.ToString()));
                        if (
                            Database.Update(
                                LogFileData.DBStructure.FilePositionsTable.TableName + Player,
                                dbfields2,
                                LogFileData.DBStructure.FilePositionsTable.LogName + "='" + this.LogName + "' AND " + LogFileData.DBStructure.FilePositionsTable.Day + "='" + keyvalue.Key.ToString() + "'"
                                ) == false)
                        {
                            Debug.WriteLine("update catch");
                            Database.Insert(LogFileData.DBStructure.FilePositionsTable.TableName + Player, dbfields2);
                        }
                    }
                }
                this.requiresSave = false;
            }

            public bool LoadFromDB()
            {
                DataTable data = Database.GetDataTable(
                    LogFileData.DBStructure.CacheTable.TableName + Player,
                    LogFileData.DBStructure.CacheTable.LogName + "='" + this.LogName + "'");

                if (data.Rows.Count > 1) throw new Exception("Woah! duplicate entries?");
                else if (data.Rows.Count == 0)
                    return false;

                DataRow row = data.Rows[0];
                this.LogName = row[LogFileData.DBStructure.CacheTable.LogName].ToString();
                try
                {
                    this.LogDate = Convert.ToDateTime(row[LogFileData.DBStructure.CacheTable.LogDate], CultureInfo.InvariantCulture);
                }
                catch (FormatException exception)
                {
                    try
                    {
                        this.LogDate = Convert.ToDateTime(row[LogFileData.DBStructure.CacheTable.LogDate]);
                    }
                    catch (FormatException secondException)
                    {
                        Logger.LogError(string.Format("Parsing LogDate failed, log: {0}, raw value: {1}. To fix this problem, try: options - rebuild logs cache.",
                            this.LogName, row[LogFileData.DBStructure.CacheTable.LogDate]),
                            THIS, secondException);
                        throw;
                    }
                }
                this.LogType = GameLogTypesEX.GetLogTypeForName(row[LogFileData.DBStructure.CacheTable.LogType].ToString());
                this.PlayerPM = row[LogFileData.DBStructure.CacheTable.PlayerPM].ToString();
                this.FilePath = row[LogFileData.DBStructure.CacheTable.FilePath].ToString();
                this.isMonthly = Convert.ToBoolean(row[LogFileData.DBStructure.CacheTable.isMonthly]);
                this.LastFileSize = Convert.ToInt64(row[LogFileData.DBStructure.CacheTable.LastFileSize]);

                if (this.isMonthly)
                {
                    MonthlyFileToDayPositionMap = new Dictionary<int, int>();
                    DataTable data2 = Database.GetDataTable(
                        LogFileData.DBStructure.FilePositionsTable.TableName + Player,
                        LogFileData.DBStructure.FilePositionsTable.LogName + "='" + this.LogName + "'");
                    foreach (DataRow row2 in data2.Rows)
                    {
                        MonthlyFileToDayPositionMap.Add(
                            Convert.ToInt32(row2[LogFileData.DBStructure.FilePositionsTable.Day]),
                            Convert.ToInt32(row2[LogFileData.DBStructure.FilePositionsTable.FilePosition]));
                    }
                }

                return true;
            }

            /// <summary>
            /// Returns line index, that is ending provided day entries block. Returns -1 if this is last block.
            /// </summary>
            /// <param name="day">Day for which to find ending line index</param>
            /// <returns></returns>
            public int GetLastLinePosForThisStartPosition(int day)
            {
                int endPos;
                int iteratorDay = day + 1; //init from next day
                while (iteratorDay < 32) //loop until last day in longest month
                {
                    if (MonthlyFileToDayPositionMap.TryGetValue(iteratorDay, out endPos))
                    {
                        return endPos - 1; //return correct end position if entry found
                    }
                    iteratorDay++; //continue iterating days
                }
                return -1; //if nothing found, return default -1
            }
        }

        List<LogFileData> AllLogFiles = new List<LogFileData>();
        Dictionary<string, LogFileData> PathToLogFileDataMap = new Dictionary<string, LogFileData>();
        string Player;
        string LogFilesPath;
        SQLiteDB Database;
        public bool incorrectLogsDir = false;

        public LogFileSearcherV2(string logFilesPath, SQLiteDB database)
        {
            this.LogFilesPath = logFilesPath;
            this.Database = database;
            Player = GeneralHelper.GetPreviousDirNameFromDirPath(logFilesPath);
            PrepareDatabaseTables();
            CacheAllFiles();
        }

        void CacheAllFiles()
        {
            Logger.__WriteLine("LogSearcher: Building log cache for " + this.Player);
            try
            {
                string[] files = Directory.GetFiles(LogFilesPath);
                // inform user of progress
                foreach (string file in files)
                {
                    LogFileData logfiledata = new LogFileData(file, Player, Database);
                    if (!logfiledata.isUnrecognizedFile)
                    {
                        AllLogFiles.Add(logfiledata);
                        PathToLogFileDataMap.Add(file, logfiledata);
                    }
                }
                Logger.__WriteLine("LogSearcher: Found " + AllLogFiles.Count + " logs");

                Logger.__WriteLine("LogSearcher: Saving cache to database");
                TransSaveAllNewFilesToDB();
            }
            catch (IOException _e)
            {
                Logger.LogInfo("could not init logs cache for path: " + LogFilesPath, THIS, _e);
                incorrectLogsDir = true;
            }
            catch (Exception _e)
            {
                Logger.LogError("something went wrong at CacheAllFiles", THIS, _e);
            }
        }

        void PrepareDatabaseTables()
        {
            //create table if not exists
            Debug.WriteLine("LogFileSearcher: Preparing database...");
            Database.CreateTable(
                LogFileData.DBStructure.CacheTable.TableName + Player,
                LogFileData.DBStructure.CacheTable.FieldsArray,
                true);
            Database.CreateTable(
                LogFileData.DBStructure.FilePositionsTable.TableName + Player,
                LogFileData.DBStructure.FilePositionsTable.FieldsArray,
                true);
            Debug.WriteLine("LogFileSearcher: Finished preparing database");
        }

        void TransSaveAllNewFilesToDB()
        {
            try
            {
                Database.BeginTrans();

                int numOfSavedLogs = 0;
                foreach (LogFileData logdata in AllLogFiles)
                {
                    if (logdata.requiresSave)
                    {
                        logdata.SaveToDB();
                        numOfSavedLogs++;
                    }

                }

                Database.CommitTrans();
                if (numOfSavedLogs > 0) Debug.WriteLine("LogFileSearcher: Saving completed successfully");
            }
            catch (Exception _e)
            {
                Database.RollbackTrans();
                Debug.WriteLine("DB inserts failed");
                Logger.LogError("!!! LogSearcher: Saving failed! This may be a bug! Please try again by restarting the program", THIS, _e);
            }
        }

        public void UpdateCache()
        {
            if (!incorrectLogsDir)
            {
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(LogFilesPath);
                    foreach (string file in files)
                    {
                        if (!PathToLogFileDataMap.Keys.Contains(file))
                        {
                            LogFileData newlogfiledata = new LogFileData(file, this.Player, Database);
                            if (!newlogfiledata.isUnrecognizedFile)
                            {
                                AllLogFiles.Add(newlogfiledata);
                                PathToLogFileDataMap.Add(file, newlogfiledata);
                                Debug.WriteLine("found and cached new file");
                            }
                        }
                        else
                        {
                            FileInfo fileinfo = new FileInfo(file);
                            LogFileData logfiledata;
                            if (PathToLogFileDataMap.TryGetValue(file, out logfiledata))
                            {
                                if (logfiledata.LastFileSize != fileinfo.Length)
                                {
                                    logfiledata.Update();
                                    logfiledata.LastFileSize = fileinfo.Length;
                                    logfiledata.requiresSave = true;
                                    Debug.WriteLine("updated cached file!!");
                                }
                            }
                        }
                    }
                    TransSaveAllNewFilesToDB();
                }
                catch (IOException _e)
                {
                    Logger.LogInfo("could not update logs cache for path: " + LogFilesPath, THIS, _e);
                    incorrectLogsDir = true;
                }
                catch (Exception _e)
                {
                    Logger.LogError("something went wrong at UpdateCache", THIS, _e);
                }
            }
        }

        public void Abort()
        {
            try
            {
                Database.TerminateConnection();
            }
            catch
            {
            }
        }

        public void ForceRecacheAllData()
        {
            Database.ClearTable(LogFileData.DBStructure.CacheTable.TableName + Player);
            Database.ClearTable(LogFileData.DBStructure.FilePositionsTable.TableName + Player);
            AllLogFiles.Clear();
            PathToLogFileDataMap.Clear();
            CacheAllFiles();
        }

        class DateToLogsMap : IComparable<DateToLogsMap>
        {
            public DateTime Date;
            public LogFileData LogFileDataObj;
            public int FileStartPosition = -1;
            public int FileEndPosition = -1;

            public DateToLogsMap(DateTime date, LogFileData logfiledata, int fileposition = -1, int fileendposition = -1)
            {
                this.Date = date;
                this.LogFileDataObj = logfiledata;
                this.FileStartPosition = fileposition;
                this.FileEndPosition = fileendposition;
            }

            public int CompareTo(DateToLogsMap dtlm)
            {
                return this.Date.CompareTo(dtlm.Date);
            }
        }

        #region SEARCH HANDLING

        public LogSearchData GetFilteredSearchList(GameLogTypes gamelogtype, DateTime timefrom, DateTime timeto, LogSearchData logsearchdata)
        {
            // implement search
            //get the list of logs containing all the requested data
            List<LogFileData> matchingLogFiles = new List<LogFileData>();
            List<DateToLogsMap> dateToLogsMapList = new List<DateToLogsMap>();

            bool isSearchKeyEmpty = false;

            if (logsearchdata.SearchCriteria.SearchKey.Trim() == "")
            {
                isSearchKeyEmpty = true;
            }

            foreach (LogFileData logfiledata in AllLogFiles)
            {
                if (logfiledata.LogType == gamelogtype && !logsearchdata.StopSearching)
                {
                    DateTime adjustedTimeFrom;

                    if (logfiledata.isMonthly)
                    {
                        adjustedTimeFrom = new DateTime(timefrom.Year, timefrom.Month, 1);
                    }
                    else
                    {
                        adjustedTimeFrom = timefrom;
                    }

                    bool isPM_Valid = true;
                    if (logsearchdata.SearchCriteria.PM_Player != null)
                    {
                        try
                        {
                            isPM_Valid = Regex.IsMatch(logfiledata.PlayerPM, "(?i)" + logsearchdata.SearchCriteria.PM_Player);
                        }
                        catch
                        {
                            isPM_Valid = false;
                        }
                    }

                    if (logfiledata.LogDate >= adjustedTimeFrom && logfiledata.LogDate <= timeto && isPM_Valid)
                    {
                        Debug.WriteLine("match: " + logfiledata.LogName + " date: " + logfiledata.LogDate + " ismonthly: " + logfiledata.isMonthly);
                        if (logfiledata.isMonthly)
                        {
                            foreach (var keyval in logfiledata.MonthlyFileToDayPositionMap)
                            {
                                DateTime thisKeyDate;
                                try
                                {
                                    thisKeyDate = new DateTime(logfiledata.LogDate.Year, logfiledata.LogDate.Month, keyval.Key);
                                }
                                catch (ArgumentOutOfRangeException) //verify if no value is greater than absolute max for datetime
                                {
                                    int year, month, day;
                                    year = logfiledata.LogDate.Year;
                                    month = logfiledata.LogDate.Month;
                                    day = keyval.Key;
                                    Logger.__WriteLine("!LogSearcher result preparation error for monthly log: " + logfiledata.LogName + " ; datetime was out of range: " + day + " " + month + " " + year);
                                    if (year < 1) year = 1;
                                    if (year > 9999) year = 9999;
                                    if (month < 1) month = 1;
                                    if (month > 12) month = 12;
                                    if (day < 1) day = 1;
                                    if (day > DateTime.DaysInMonth(year, month)) day = DateTime.DaysInMonth(year, month);
                                    thisKeyDate = new DateTime(year, month, day);
                                }
                                if (thisKeyDate >= timefrom && thisKeyDate <= timeto)
                                {
                                    dateToLogsMapList.Add(new DateToLogsMap(
                                        new DateTime(thisKeyDate.Year, thisKeyDate.Month, thisKeyDate.Day),
                                        logfiledata,
                                        keyval.Value,
                                        logfiledata.GetLastLinePosForThisStartPosition(thisKeyDate.Day)));
                                    Debug.WriteLine("Added day " + thisKeyDate.Day + " filepos: " + keyval.Value);
                                }
                            }
                        }
                        else
                        {
                            dateToLogsMapList.Add(new DateToLogsMap(logfiledata.LogDate, logfiledata));
                        }
                    }
                }
            }

            //build a chronological string list from these logs
            Debug.WriteLine("Sorting, postsort");
            dateToLogsMapList.Sort();
            int lastMonthlyMonth = -1;
            TextFileObject monthlyTextFileObject = null;
            long currentLineStartIndex = 0;
            //build the output list, find matches
            foreach (DateToLogsMap datetologsmap in dateToLogsMapList)
            {
                if (!logsearchdata.StopSearching)
                {
                    Debug.WriteLine(datetologsmap.Date + " lfile: " + datetologsmap.LogFileDataObj.LogName + " ldate: " + datetologsmap.LogFileDataObj.LogDate + " lpos: " + datetologsmap.FileStartPosition);
                    if (datetologsmap.LogFileDataObj.isMonthly)
                    {
                        if (logsearchdata.SearchCriteria.GameLogType == GameLogTypes.PM && logsearchdata.SearchCriteria.PM_Player == null)
                        {
                            monthlyTextFileObject = new TextFileObject(
                                datetologsmap.LogFileDataObj.FilePath,
                                true, true, false, true, false, false, newLineOnlyOnCRLF: true);

                            string emptyAdd = " ";
                            logsearchdata.AllLines.Add(emptyAdd);
                            currentLineStartIndex += emptyAdd.Length + 1;

                            string pmConvWith = "  ### PM conversation with: " + datetologsmap.LogFileDataObj.PlayerPM;
                            logsearchdata.AllLines.Add(pmConvWith);
                            currentLineStartIndex += pmConvWith.Length + 1;
                        }
                        else if (lastMonthlyMonth != datetologsmap.Date.Month)
                        {
                            lastMonthlyMonth = datetologsmap.Date.Month;
                            monthlyTextFileObject = new TextFileObject(
                                datetologsmap.LogFileDataObj.FilePath,
                                true, true, false, true, false, false, newLineOnlyOnCRLF: true);
                        }

                        int readLineNumber = datetologsmap.FileStartPosition;
                        int readEndLineNumber = datetologsmap.FileEndPosition;
                        bool noMoreDaysInThisFile = false;
                        if (readEndLineNumber == -1) noMoreDaysInThisFile = true;

                        while (readLineNumber <= readEndLineNumber || noMoreDaysInThisFile)
                        {
                            string line = monthlyTextFileObject.ReadLine(readLineNumber);

                            if (line == null)
                            {
                                break;
                            }
                            else
                            {
                                line = AppendDateToLine(line, datetologsmap.Date);
                                logsearchdata.AllLines.Add(line);
                                if (!isSearchKeyEmpty) FindMatch(logsearchdata, line, currentLineStartIndex);
                                currentLineStartIndex += line.Length + 1; // offset for richtextbox (endline?)
                            }
                            readLineNumber++;
                        }
                    }
                    else
                    {
                        try
                        {
                            using (FileStream fs = new FileStream(datetologsmap.LogFileDataObj.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                using (StreamReader sr = new StreamReader(fs, Encoding.Default))
                                {
                                    string line;
                                    while ((line = sr.ReadLine()) != null)
                                    {
                                        line = AppendDateToLine(line, datetologsmap.Date);
                                        logsearchdata.AllLines.Add(line);
                                        if (!isSearchKeyEmpty) FindMatch(logsearchdata, line, currentLineStartIndex);
                                        currentLineStartIndex += line.Length + 1; // offset for richtextbox
                                    }
                                }
                            }
                        }
                        catch (Exception _e)
                        {
                            Logger.LogError("Problem reading a log file: "+ (datetologsmap.LogFileDataObj.FilePath ?? "NULL"), THIS, _e);
                        }
                    }
                }
            }

            //Debug.WriteLine("Result all lines");
            //foreach (string line in logsearchdata.AllLines)
            //{
            //    Debug.WriteLine(line);
            //}
            //Debug.WriteLine("END Result all lines");

            //assing the list to Result and return Result
            return logsearchdata;
        }

        string AppendDateToLine(string line, DateTime date)
        {
            if (line.StartsWith("[", StringComparison.Ordinal))
            {
                line = date.ToString("[yyyy-MM-dd] ") + line;
            }
            return line;
        }

        void FindMatch(LogSearchData logsearchdata, string line, long currentLineStartIndex)
        {
            //search the line for matches
            if (logsearchdata.SearchCriteria.SearchType == SearchTypes.RegexEscapedCaseIns)
            {
                string escapedPattern = Regex.Escape(logsearchdata.SearchCriteria.SearchKey);
                try
                {
                    MatchCollection matchcollection = Regex.Matches(line, "(?i)" + escapedPattern);
                    foreach (Match match in matchcollection)
                    {
                        long matchStart = currentLineStartIndex + match.Index;
                        long matchLength = match.Length;

                        logsearchdata.SearchResults.Add(
                            new LogSearchData.SingleSearchMatch(matchStart, matchLength, BuildDateForMatch(line)));
                    }
                }
                catch
                {
                    Debug.WriteLine("FindMatch exc RegexEscapedCaseIns");
                }
            }
            else if (logsearchdata.SearchCriteria.SearchType == SearchTypes.RegexCustom)
            {
                try
                {
                    MatchCollection matchcollection = Regex.Matches(line, logsearchdata.SearchCriteria.SearchKey);
                    foreach (Match match in matchcollection)
                    {
                        long matchStart = currentLineStartIndex + match.Index;
                        long matchLength = match.Length;

                        logsearchdata.SearchResults.Add(
                            new LogSearchData.SingleSearchMatch(matchStart, matchLength, BuildDateForMatch(line)));
                    }
                }
                catch
                {
                    Debug.WriteLine("FindMatch exc RegexCustom");
                }
            }
        }

        DateTime BuildDateForMatch(string line)
        {
            return LogSearchManager.BuildDateForMatch(line);

            //DateTime matchDate;
            //try
            //{
            //    matchDate = new DateTime(
            //        Convert.ToInt32(line.Substring(1, 4)),
            //        Convert.ToInt32(line.Substring(6, 2)),
            //        Convert.ToInt32(line.Substring(9, 2)),
            //        Convert.ToInt32(line.Substring(14, 2)),
            //        Convert.ToInt32(line.Substring(17, 2)),
            //        Convert.ToInt32(line.Substring(20, 2))
            //        );
            //}
            //catch
            //{
            //    matchDate = new DateTime(0);
            //}
            //return matchDate;
        }

        #endregion SEARCH HANDLING
    }
}
