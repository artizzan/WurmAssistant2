using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.SqlServer.Server;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Forms;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmState
{
    using SysTimer = System.Timers.Timer;
    /// <summary>
    /// Abstracts information about Wurm game client settings and provides limited means to adjust them.
    /// </summary>
    public static class WurmClient
    {
        const string THIS = "WurmClient";

        static bool _initAttempted = false;
        static bool _initSuccessful = false;
        /// <summary>
        /// true if all subsystems of WurmClient initializated without error.
        /// </summary>
        public static bool InitSuccessful
        {
            get { return _initSuccessful; }
        }

        static WurmClient()
        {
            _initSuccessful = InitializeAllSystems();
            if (!_initSuccessful)
                Logger.LogInfo("autoinit: " + _initSuccessful, THIS);
        }

        /// <summary>
        /// Overrides automatically detected wurm directory with specified path, then reinitializes WurmClient
        /// </summary>
        /// <param name="dirPath">null to reset default</param>
        /// <returns></returns>
        public static bool OverrideWurmDir(string dirPath)
        {
            WurmPaths.WurmDirManager.WurmDirManualOverride = dirPath;
            if (InitializeAllSystems()) return true; //else init and check if correct
            else return false;
        }

        /// <summary>
        /// Rebuilds all WurmClient data
        /// </summary>
        /// <returns></returns>
        public static bool Reinitialize()
        {
            _initSuccessful = InitializeAllSystems();
            Logger.LogInfo("init: " + _initSuccessful, THIS);
            return _initSuccessful;
        }

        internal static void InitIfNotInited()
        {
            if (!_initAttempted)
            {
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(WurmClient).TypeHandle);
                _initAttempted = true;
            }
        }

        static bool InitializeAllSystems()
        {
            bool initresult = true;

            //order of initializations is very important
            //todo: this needs rewrite

            if (!WurmPaths.Init()) initresult = false;

            if (!Configs.Init()) initresult = false;
            //relies on:
            //WurmPaths.GameSettingsFiles <= WurmPaths.WurmDir

            if (!PlayerConfigurations.Init()) initresult = false;
            //relies on:
            //WurmPaths.PlayersDir <= WurmPaths.WurmDir
            //Configs.GetConfig(name) <= Configs.Init()

            if (!Autoexecs.Init()) initresult = false;

            _initSuccessful = initresult;
            return initresult;
        }

        /// <summary>
        /// Provides information about Wurm game client current state
        /// </summary>
        public static class State
        {
            const string THIS = "WurmClient::State";

            static State()
            {
                WurmClient.InitIfNotInited();
            }

            public enum EnumWurmClientStatus { Unknown, Running, NotRunning }

            /// <summary>
            /// true if at least one wurm client is currently running, false if not or if checking failed
            /// </summary>
            public static EnumWurmClientStatus WurmClientRunning
            {
                get
                {
                    try
                    {
                        Process[] allActiveProcesses = Process.GetProcessesByName("javaw");
                        foreach (var process in allActiveProcesses)
                        {
                            if (process.MainWindowTitle.StartsWith("Wurm Online", StringComparison.Ordinal))
                                return EnumWurmClientStatus.Running;
                        }
                        return EnumWurmClientStatus.NotRunning;
                    }
                    catch (Exception _e)
                    {
                        Logger.LogError("There was an error while checking for active Wurm client", THIS, _e);
                        return EnumWurmClientStatus.Unknown;
                    }
                }
            }
        }

        /// <summary>
        /// Provides means to obtain accurate wurm directory and file paths
        /// </summary>
        public static class WurmPaths
        {
            const string THIS = "WurmClient::WurmPaths";

            static WurmPaths()
            {
                WurmClient.InitIfNotInited();
            }

            internal static bool Init()
            {
                bool initsuccess = true;
                if (!WurmDirManager.AutoInit()) initsuccess = false;
                if (!PlayerDirsManager.AutoInit()) initsuccess = false;
                if (!ConfigsManager.AutoInit()) initsuccess = false;
                return initsuccess;
            }

            /// <summary>
            /// handles wurm main directory path
            /// </summary>
            public static class WurmDirManager
            {
                const string THIS = "WurmDirManager";

                static string _cachedWurmOnlineDirFromRegistry;
                internal static string WurmDirManualOverride;

                internal static bool AutoInit()
                {
                    if (GetWurmDir() != null) return true;
                    else return false;
                }

                static bool CacheWurmDirFromRegistry()
                {
                    object regObj = Registry.GetValue(@"HKEY_CURRENT_USER\Software\JavaSoft\Prefs\com\wurmonline\client", "wurm_dir", null);
                    if (regObj == null)
                    {
                        Logger.LogError("Error while retrieving wurm dir from registry", THIS);
                        return false;
                    }

                    try
                    {
                        var wurmdir = Convert.ToString(regObj);
                        wurmdir = wurmdir.Replace(@"//", @"\");
                        wurmdir = wurmdir.Replace(@"/", @"");
                        wurmdir = wurmdir.Trim();
                        if (!wurmdir.EndsWith(@"\", StringComparison.Ordinal)) wurmdir += @"\";
                        _cachedWurmOnlineDirFromRegistry = wurmdir;
                        return true;
                    }
                    catch (Exception _e)
                    {
                        Logger.LogError("Registry value parse error", THIS, _e);
                        return false;
                    }
                }

                internal static string GetWurmDir()
                {
                    return WurmDirManualOverride;
                }

                public static string TryGetWoRegistryPath()
                {
                    if (_cachedWurmOnlineDirFromRegistry == null)
                    {
                        CacheWurmDirFromRegistry();
                    }
                    return _cachedWurmOnlineDirFromRegistry;
                }
            }

            /// <summary>
            /// handles wurm player directories paths
            /// </summary>
            static class PlayerDirsManager
            {
                static Dictionary<string, string> _playerToLogsPathMap = new Dictionary<string, string>();

                internal static bool AutoInit()
                {
                    return BuildPlayersToLogsPathMap();
                }

                internal static bool BuildPlayersToLogsPathMap()
                {
                    // create new dict and replace on completion to keep code thread safe
                    var newDict = new Dictionary<string, string>();
                    try
                    {
                        string[] allplayers = Directory.GetDirectories(Path.Combine(WurmDir, @"players\"));
                        foreach (var playerpath in allplayers)
                        {
                            string playername = GeneralHelper.GetLastDirNamefromDirPath(playerpath);
                            var dir = Path.Combine(playerpath, @"test_logs\");
                            var info = new DirectoryInfo(dir);
                            if (!info.Exists)
                            {
                                Logger.LogError(
                                    "A player directory appears to be invalid, skipping. Directory path: "
                                    + info.FullName,
                                    THIS);
                            }
                            else
                            {
                                newDict.Add(playername, dir);
                            }
                        }
                        if (newDict.Count > 0)
                        {
                            // ref swap is always thread safe, but may not be immediatelly reflected in all cpu caches
                            // (although .NET docs claim it does)
                            // regardless in this case it does not matter much
                            _playerToLogsPathMap = newDict;
                            return true;
                        }
                        else
                        {
                            Logger.LogError("No player directories detected on PathMap rebuild!", THIS);
                            return false;
                        }
                    }
                    catch (Exception _e)
                    {
                        Logger.LogError("Error while searching for player directories", THIS, _e);
                        return false;
                    }
                }

                internal static string[] GetAllPlayers()
                {
                    BuildPlayersToLogsPathMap();
                    return _playerToLogsPathMap.Keys.ToArray<string>();
                }

                internal static string GetLogsDirForPlayer(string player)
                {
                    try
                    {
                        return _playerToLogsPathMap[player];
                    }
                    catch
                    {
                        try
                        {
                            BuildPlayersToLogsPathMap();
                            return _playerToLogsPathMap[player];
                        }
                        catch (Exception _ee)
                        {
                            Logger.LogError("Error while retrieving logs dir for player name", THIS, _ee);
                            return null;
                        }
                    }
                }

                static bool checkIfPlayerExists(string player)
                {
                    if (_playerToLogsPathMap == null) return false;
                    if (_playerToLogsPathMap.ContainsKey(player)) return true;
                    return false;
                }

                internal static bool DoesPlayerExist(string player)
                {
                    if (checkIfPlayerExists(player)) return true;
                    else
                    {
                        BuildPlayersToLogsPathMap();
                        if (checkIfPlayerExists(player)) return true;
                    }
                    return false;
                }
            }

            static class ConfigsManager
            {
                /// <summary>
                /// Returns array of absolute paths to config directories, or null if failed
                /// </summary>
                /// <returns></returns>
                internal static string[] GetConfigsDirs()
                {
                    try
                    {
                        string configDirPath = Path.Combine(WurmDir, "configs");
                        return Directory.GetDirectories(configDirPath);
                    }
                    catch (Exception _e)
                    {
                        Logger.LogError("", THIS, _e);
                        return null;
                    }
                }

                internal static string[] GetGameSettingsFilePaths()
                {
                    var result = new List<string>();

                    string[] allconfigdirs = GetConfigsDirs();
                    if (allconfigdirs != null)
                    {
                        foreach (string dir in allconfigdirs)
                        {
                            string gamesetFile = Path.Combine(dir, "gamesettings.txt");
                            if (File.Exists(gamesetFile)) result.Add(gamesetFile);
                        }
                    }

                    if (result.Count == 0) return null;
                    else return result.ToArray();
                }

                internal static bool AutoInit()
                {
                    //simply verify this does not return nulls
                    if (GetConfigsDirs() == null) return false;
                    if (GetGameSettingsFilePaths() == null) return false;
                    return true;
                }
            }

            /// <summary>
            /// full path to directory holding all individual player directories
            /// </summary>
            public static string PlayersDir
            {
                get { return Path.Combine(WurmDir, "players"); }
            }

            /// <summary>
            /// full paths to all gamesettings.txt files or null if none found
            /// </summary>
            public static string[] GetGameSettingsFilesPaths()
            {
                return ConfigsManager.GetGameSettingsFilePaths();
            }

            /// <summary>
            /// full paths to all config directories or null if none found
            /// </summary>
            public static string[] GetConfigDirs()
            {
                return ConfigsManager.GetConfigsDirs();
            }

            /// <summary>
            /// full path to client top directory or null if none found
            /// </summary>
            public static string WurmDir
            {
                get { return WurmDirManager.GetWurmDir(); }
            }

            /// <summary>
            /// all wurm character dir names found within wurm directory
            /// </summary>
            public static string[] GetAllPlayersNames()
            {
                return PlayerDirsManager.GetAllPlayers();
            }

            /// <summary>
            /// full path to log directory for this wurm character if exists, else null
            /// </summary>
            /// <param name="player">player name case sensitive</param>
            /// <returns></returns>
            public static string GetLogsDirForPlayer(string player)
            {
                return PlayerDirsManager.GetLogsDirForPlayer(player);
            }

            /// <summary>
            /// true if this wurm character directory exists.
            /// </summary>
            /// <param name="player">player name case sensitive</param>
            /// <returns></returns>
            public static bool PlayerExists(string player)
            {
                return PlayerDirsManager.DoesPlayerExist(player);
            }

            /// <summary>
            /// path to playerdata.txt for this player
            /// </summary>
            /// <param name="player">player name case sensitive</param>
            /// <returns></returns>
            public static string GetFilePathForPlayerData(string player)
            {
                return Path.Combine(PlayersDir, player, "playerdata.txt");
            }

            public static string GetPlayerDir(string player)
            {
                return Path.Combine(PlayersDir, player);
            }
        }

        /// <summary>
        /// Provides means to interact with configs present in wurm client directory. 
        /// Autoupdates information about the config.
        /// NOTE: Editing configs with this class may break other wurm tools and is not recommended.
        /// </summary>
        public static class Configs
        {
            const string THIS = "WurmClient::Configs";

            static Configs()
            {
                WurmClient.InitIfNotInited();
            }

            public enum EnumFileSource { Unknown, ProfileFolder, PlayerFolder };
            public enum EnumLoggingType { Unknown, Never, OneFile, Monthly, Daily };
            public enum EnumSkillGainRate { Unknown, Never, PerInteger, Per0_1, Per0_01, per0_001, Always }

            /// <summary>
            /// Provides information about single gamesettings.txt
            /// </summary>
            public class ConfigData : IDisposable
            {
                private readonly string _configFilePath;
                private readonly FileSystemWatcher _changeWatcher;
                private readonly string _configDir;

                public string ConfigDir
                {
                    get
                    {
                        return _configDir;
                    }
                }

                internal ConfigData(string configPath)
                {
                    _configFilePath = configPath;
                    _configDir = Path.GetDirectoryName(configPath);
                    LoadAllData();
                    _changeWatcher = new FileSystemWatcher(_configDir);
                    _changeWatcher.Filter = "gamesettings.txt";
                    _changeWatcher.Changed += new FileSystemEventHandler(OnFSWevent);
                    _changeWatcher.EnableRaisingEvents = true;
                }

                /// <summary>
                /// this config name
                /// </summary>
                public string ConfigName
                {
                    get
                    {
                        return GeneralHelper.GetLastDirNamefromDirPath(Path.GetDirectoryName(_configFilePath));
                    }
                }

                EnumFileSource _CustomTimerSource = EnumFileSource.Unknown;
                /// <summary>
                /// 'custom timer source' value in this config
                /// </summary>
                public EnumFileSource CustomTimerSource
                {
                    get { return _CustomTimerSource; }
                }

                EnumFileSource _ExecSource = EnumFileSource.Unknown;
                /// <summary>
                /// 'exec source' value in this config
                /// </summary>
                public EnumFileSource ExecSource
                {
                    get { return _ExecSource; }
                }

                EnumFileSource _KeyBindSource = EnumFileSource.Unknown;
                /// <summary>
                /// 'keybindings source' value in this config
                /// </summary>
                public EnumFileSource KeyBindSource
                {
                    get { return _KeyBindSource; }
                }

                EnumFileSource _AutoRunSource = EnumFileSource.Unknown;
                /// <summary>
                /// 'autorun source' value in this config
                /// </summary>
                public EnumFileSource AutoRunSource
                {
                    get { return _AutoRunSource; }
                }

                EnumLoggingType _IrcLoggingType = EnumLoggingType.Unknown;
                /// <summary>
                /// 'irc message logging' value in this config
                /// </summary>
                public EnumLoggingType IrcLoggingType
                {
                    get { return _IrcLoggingType; }
                }

                EnumLoggingType _OtherLoggingType = EnumLoggingType.Unknown;
                /// <summary>
                /// 'other message logging' value in this config
                /// </summary>
                public EnumLoggingType OtherLoggingType
                {
                    get { return _OtherLoggingType; }
                }

                EnumLoggingType _EventLoggingType = EnumLoggingType.Unknown;
                /// <summary>
                /// 'event message logging' value in this config
                /// </summary>
                public EnumLoggingType EventLoggingType
                {
                    get { return _EventLoggingType; }
                }

                /// <summary>
                /// returns true if all 3 logging mode options are equal to the same value 
                /// and the value is present in argument array
                /// </summary>
                /// <param name="loggingModes"></param>
                /// <returns></returns>
                public bool EventAndOtherLoggingModesAreEqual(EnumLoggingType[] loggingModes)
                {
                    if (EventLoggingType == OtherLoggingType) //&& OtherLoggingType == IrcLoggingType) // irc is not used for anything
                    {
                        if (loggingModes.Contains(EventLoggingType))
                            return true;
                    }
                    return false;
                }

                /// <summary>
                /// Sets 'Other and event logging' to specified value in this config, 
                /// requires that no wurm clients are running, else will abort
                /// </summary>
                /// <param name="newValue"></param>
                /// <returns></returns>
                public bool SetCommonLoggingMode(EnumLoggingType newValue)
                {
                    try
                    {
                        if (State.WurmClientRunning == State.EnumWurmClientStatus.Running) return false;
                        if (newValue == EnumLoggingType.Unknown) return false;
                        //string replacementIRC = "irc_log_rotation=";  //irc is not used for anything and users may not like irc logging forced on them
                        string replacementOTHER = "other_log_rotation=";
                        string replacementEVENT = "event_log_rotation=";
                        if (newValue == EnumLoggingType.Never)
                        {
                            //replacementIRC += "0";
                            replacementOTHER += "0";
                            replacementEVENT += "0";
                        }
                        if (newValue == EnumLoggingType.OneFile)
                        {
                            //replacementIRC += "1";
                            replacementOTHER += "1";
                            replacementEVENT += "1";
                        }
                        if (newValue == EnumLoggingType.Monthly)
                        {
                            //replacementIRC += "2";
                            replacementOTHER += "2";
                            replacementEVENT += "2";
                        }
                        if (newValue == EnumLoggingType.Daily)
                        {
                            //replacementIRC += "3";
                            replacementOTHER += "3";
                            replacementEVENT += "3";
                        }
                        bool result = true;

                        //if (RewriteFile(@"irc_log_rotation=\d", replacementIRC))
                        //    _IrcLoggingType = newValue;
                        //else result = false;
                        if (RewriteFile(@"other_log_rotation=\d", replacementOTHER))
                            _OtherLoggingType = newValue;
                        else result = false;
                        if (RewriteFile(@"event_log_rotation=\d", replacementEVENT))
                            _EventLoggingType = newValue;
                        else result = false;

                        return result;
                    }
                    catch (Exception _e)
                    {
                        Logger.LogError("Error while modifying settings in wurm config file", this, _e);
                        return false;
                    }
                }

                EnumSkillGainRate _SkillGainRate = EnumSkillGainRate.Unknown;
                /// <summary>
                /// 'skillgain tab updates' value in this config
                /// </summary>
                public EnumSkillGainRate SkillGainRate
                {
                    get { return _SkillGainRate; }
                }

                /// <summary>
                /// Sets 'skillgain tab updates' to specified value in this config,
                /// requires that no wurm clients are running, else will abort
                /// </summary>
                /// <param name="newValue"></param>
                /// <returns></returns>
                public bool SetSkillGainRate(EnumSkillGainRate newValue)
                {
                    try
                    {
                        if (State.WurmClientRunning == State.EnumWurmClientStatus.Running) return false;
                        if (newValue == EnumSkillGainRate.Unknown) return false;
                        string replacement = "skillgain_minimum=";
                        if (newValue == EnumSkillGainRate.Never) replacement += "0";
                        if (newValue == EnumSkillGainRate.PerInteger) replacement += "1";
                        if (newValue == EnumSkillGainRate.Per0_1) replacement += "2";
                        if (newValue == EnumSkillGainRate.Per0_01) replacement += "3";
                        if (newValue == EnumSkillGainRate.per0_001) replacement += "4";
                        if (newValue == EnumSkillGainRate.Always) replacement += "5";
                        if (RewriteFile(@"skillgain_minimum=\d", replacement))
                        {
                            _SkillGainRate = newValue;
                            return true;
                        }
                        else return false;
                    }
                    catch (Exception _e)
                    {
                        Logger.LogError("Error while modifying settings in wurm config file", this, _e);
                        return false;
                    }
                }

                bool? _NoSkillMessageOnAlignmentChange = null;
                /// <summary>
                /// 'hide alignment updates' value in this config, null if unknown
                /// </summary>
                public bool? NoSkillMessageOnAlignmentChange
                {
                    get { return _NoSkillMessageOnAlignmentChange; }
                }

                /// <summary>
                /// Sets 'hide alignment updates' to specified value in this config, 
                /// requires that no wurm clients are running, else will abort
                /// </summary>
                /// <param name="newValue"></param>
                /// <returns></returns>
                public bool SetNoSkillMessageOnAlignmentChange(bool newValue)
                {
                    try
                    {
                        if (State.WurmClientRunning == State.EnumWurmClientStatus.Running) return false;
                        string replacement = "skillgain_no_alignment=";
                        if (newValue == false) replacement += "false";
                        if (newValue == true) replacement += "true";
                        if (RewriteFile(@"skillgain_no_alignment=\w+", replacement))
                        {
                            _NoSkillMessageOnAlignmentChange = newValue;
                            return true;
                        }
                        else return false;
                    }
                    catch (Exception _e)
                    {
                        Logger.LogError("Error while modifying settings in wurm config file", this, _e);
                        return false;
                    }
                }

                bool? _NoSkillMessageOnFavorChange = null;
                /// <summary>
                /// 'hide favor updates' value in this config,  null if unknown.
                /// </summary>
                public bool? NoSkillMessageOnFavorChange
                {
                    get { return _NoSkillMessageOnFavorChange; }
                }

                /// <summary>
                /// Sets 'hide favor updates' to specified value in this config,
                /// requires that no wurm clients are running, else will abort
                /// </summary>
                /// <param name="newValue"></param>
                /// <returns></returns>
                public bool SetNoSkillMessageOnFavorChange(bool newValue)
                {
                    try
                    {
                        if (State.WurmClientRunning == State.EnumWurmClientStatus.Running) return false;
                        string replacement = "skillgain_no_favor=";
                        if (newValue == false) replacement += "false";
                        if (newValue == true) replacement += "true";
                        if (RewriteFile(@"skillgain_no_favor=\w+", replacement))
                        {
                            _NoSkillMessageOnFavorChange = newValue;
                            return true;
                        }
                        else return false;
                    }
                    catch (Exception _e)
                    {
                        Logger.LogError("Error while modifying settings in wurm config file", this, _e);
                        return false;
                    }
                }

                bool? _SaveSkillsOnQuit = null;
                /// <summary>
                /// 'save skills on exit' value in this config, null if unknown
                /// </summary>
                public bool? SaveSkillsOnQuit
                {
                    get { return _SaveSkillsOnQuit; }
                }

                /// <summary>
                /// Sets 'save skills on exit' to specified value in this config, 
                /// requires that no wurm clients are running, else will abort
                /// </summary>
                /// <param name="newValue"></param>
                /// <returns></returns>
                public bool SetSaveSkillsOnQuit(bool newValue)
                {
                    try
                    {
                        if (State.WurmClientRunning == State.EnumWurmClientStatus.Running) return false;
                        string replacement = "save_skills_on_quit=";
                        if (newValue == false) replacement += "false";
                        if (newValue == true) replacement += "true";
                        if (RewriteFile(@"save_skills_on_quit=\w+", replacement))
                        {
                            _SaveSkillsOnQuit = newValue;
                            return true;
                        }
                        else return false;
                    }
                    catch (Exception _e)
                    {
                        Logger.LogError("Error while modifying settings in wurm config file", this, _e);
                        return false;
                    }
                }

                bool? _TimestampMessages = null;
                /// <summary>
                /// 'timestamp messages' value in this config, null if unknown
                /// </summary>
                public bool? TimestampMessages
                {
                    get { return _TimestampMessages; }
                }

                /// <summary>
                /// Sets 'timestamp messages" to specified value in this config,
                /// requires that no wurm clients are running, else will abort
                /// </summary>
                /// <param name="newValue"></param>
                /// <returns></returns>
                public bool SetTimestampMessages(bool newValue)
                {
                    try
                    {
                        if (State.WurmClientRunning == State.EnumWurmClientStatus.Running) return false;
                        string replacement = "setting_timestamps=";
                        if (newValue == false) replacement += "false";
                        if (newValue == true) replacement += "true";
                        if (RewriteFile(@"setting_timestamps=\w+", replacement))
                        {
                            _TimestampMessages = newValue;
                            return true;
                        }
                        else return false;
                    }
                    catch (Exception _e)
                    {
                        Logger.LogError("Error while modifying settings in wurm config file", this, _e);
                        return false;
                    }
                }

                object FSWevent_lock = new object();
                void OnFSWevent(object sender, FileSystemEventArgs e)
                {
                    lock (FSWevent_lock)
                    {
                        lock (configFileReadOrWrite_lock)
                        {
                            LoadAllData();
                        }
                    }
                }

                bool LoadAllData()
                {
                    try
                    {
                        using (var fs = new FileStream(_configFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (var sr = new StreamReader(fs))
                            {
                                string line;
                                while ((line = sr.ReadLine()) != null)
                                {
                                    ProcessLine(line);
                                }
                            }
                        }
                        return true;
                    }
                    catch (Exception _e)
                    {
                        Logger.LogError("Error while loading or processing wurm config data", this, _e);
                        return false;
                    }
                }

                void ProcessLine(string line)
                {
                    if (line.Contains("custim_timer_source"))
                    {
                        int? val = ExtractSettingNumericValue(line);
                        if (val == 0) _CustomTimerSource = EnumFileSource.ProfileFolder;
                        else if (val == 1) _CustomTimerSource = EnumFileSource.PlayerFolder;
                    }
                    else if (line.Contains("exec_source"))
                    {
                        int? val = ExtractSettingNumericValue(line);
                        if (val == 0) _ExecSource = EnumFileSource.ProfileFolder;
                        else if (val == 1) _ExecSource = EnumFileSource.PlayerFolder;
                    }
                    else if (line.Contains("key_bindings_source"))
                    {
                        int? val = ExtractSettingNumericValue(line);
                        if (val == 0) _KeyBindSource = EnumFileSource.ProfileFolder;
                        else if (val == 1) _KeyBindSource = EnumFileSource.PlayerFolder;
                    }
                    else if (line.Contains("auto_run_source"))
                    {
                        int? val = ExtractSettingNumericValue(line);
                        if (val == 0) _AutoRunSource = EnumFileSource.ProfileFolder;
                        else if (val == 1) _AutoRunSource = EnumFileSource.PlayerFolder;
                    }

                    else if (line.Contains("irc_log_rotation"))
                    {
                        int? val = ExtractSettingNumericValue(line);
                        if (val == 0) _IrcLoggingType = EnumLoggingType.Never;
                        else if (val == 1) _IrcLoggingType = EnumLoggingType.OneFile;
                        else if (val == 2) _IrcLoggingType = EnumLoggingType.Monthly;
                        else if (val == 3) _IrcLoggingType = EnumLoggingType.Daily;
                    }
                    else if (line.Contains("other_log_rotation"))
                    {
                        int? val = ExtractSettingNumericValue(line);
                        if (val == 0) _OtherLoggingType = EnumLoggingType.Never;
                        else if (val == 1) _OtherLoggingType = EnumLoggingType.OneFile;
                        else if (val == 2) _OtherLoggingType = EnumLoggingType.Monthly;
                        else if (val == 3) _OtherLoggingType = EnumLoggingType.Daily;
                    }
                    else if (line.Contains("event_log_rotation"))
                    {
                        int? val = ExtractSettingNumericValue(line);
                        if (val == 0) _EventLoggingType = EnumLoggingType.Never;
                        else if (val == 1) _EventLoggingType = EnumLoggingType.OneFile;
                        else if (val == 2) _EventLoggingType = EnumLoggingType.Monthly;
                        else if (val == 3) _EventLoggingType = EnumLoggingType.Daily;
                    }
                    else if (line.Contains("skillgain_minimum"))
                    {
                        int? val = ExtractSettingNumericValue(line);
                        if (val == 0) _SkillGainRate = EnumSkillGainRate.Never;
                        else if (val == 1) _SkillGainRate = EnumSkillGainRate.PerInteger;
                        else if (val == 2) _SkillGainRate = EnumSkillGainRate.Per0_1;
                        else if (val == 3) _SkillGainRate = EnumSkillGainRate.Per0_01;
                        else if (val == 4) _SkillGainRate = EnumSkillGainRate.per0_001;
                        else if (val == 5) _SkillGainRate = EnumSkillGainRate.Always;
                    }

                    else if (line.Contains("skillgain_no_alignment"))
                    {
                        bool? val = ExtractBoolValue(line);
                        if (val == true) _NoSkillMessageOnAlignmentChange = true;
                        else if (val == false) _NoSkillMessageOnAlignmentChange = false;
                    }
                    else if (line.Contains("skillgain_no_favor"))
                    {
                        bool? val = ExtractBoolValue(line);
                        if (val == true) _NoSkillMessageOnFavorChange = true;
                        else if (val == false) _NoSkillMessageOnFavorChange = false;
                    }
                    else if (line.Contains("save_skills_on_quit"))
                    {
                        bool? val = ExtractBoolValue(line);
                        if (val == true) _SaveSkillsOnQuit = true;
                        else if (val == false) _SaveSkillsOnQuit = false;
                    }
                    else if (line.Contains("setting_timestamps"))
                    {
                        bool? val = ExtractBoolValue(line);
                        if (val == true) _TimestampMessages = true;
                        else if (val == false) _TimestampMessages = false;
                    }
                }

                int? ExtractSettingNumericValue(string line)
                {
                    try
                    {
                        string settingString = Regex.Match(line, @"=(\d)").Groups[1].Value;
                        return int.Parse(settingString);
                    }
                    catch (Exception _e)
                    {
                        Logger.LogError("", this, _e);
                        return null;
                    }
                }

                bool? ExtractBoolValue(string line)
                {
                    try
                    {
                        string settingString = Regex.Match(line, @"=(\w+)").Groups[1].Value;
                        return bool.Parse(settingString);
                    }
                    catch (Exception _e)
                    {
                        Logger.LogError("", this, _e);
                        return null;
                    }
                }

                object configFileReadOrWrite_lock = new object();

                /// <summary>
                /// Rewrites gamesettings.txt , if no setting found, it is appended, 
                /// returns false on any error
                /// </summary>
                /// <param name="currentSettingRegex">Regex pattern to search for</param>
                /// <param name="replacementSettingString">String to replace found pattern</param>
                /// <returns></returns>
                bool RewriteFile(string currentSettingRegex, string replacementSettingString)
                {
                    lock (configFileReadOrWrite_lock)
                    {
                        try
                        {

                            string configText;
                            Encoding fileEncoding;
                            //save encoding to ensure correct output
                            using (FileStream fs = new FileStream(_configFilePath, FileMode.Open, FileAccess.Read, FileShare.None))
                            {
                                using (StreamReader sr = new StreamReader(fs))
                                {
                                    fileEncoding = sr.CurrentEncoding;
                                    configText = sr.ReadToEnd();
                                }
                            }

                            //need to count replacements
                            int replaceCount = 0;
                            configText = Regex.Replace(
                                configText,
                                currentSettingRegex, m =>
                                {
                                    replaceCount++;
                                    return replacementSettingString;
                                }
                                , RegexOptions.CultureInvariant);

                            //if there were replacements, rewrite the file.
                            //else use file appending and add option at the end of config
                            bool append = replaceCount > 0 ? false : true;
                            using (FileStream fs = new FileStream(_configFilePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                using (StreamWriter sw = new StreamWriter(fs, fileEncoding))
                                {
                                    if (!append) sw.Write(configText);
                                    else
                                    {
                                        //trim end whitespace
                                        configText = configText.TrimEnd(new char[] { ' ' });

                                        //verify that the file ends with correct newline
                                        //add newline if not
                                        string lastTwoChars = configText.Trim().Substring(configText.Length - 2, 2);
                                        if (lastTwoChars != "\r\n") sw.Write("\r\n");
                                        //write setting
                                        sw.Write(replacementSettingString + "\r\n");
                                    }
                                }
                            }
                            return true;
                        }
                        catch (Exception _e)
                        {
                            Logger.LogError("Error while rewriting wurm config file", this, _e);
                            return false;
                        }
                    }
                }

                public void Dispose()
                {
                    _changeWatcher.Dispose();
                }
            }

            static Dictionary<string, ConfigData> AllConfigs = new Dictionary<string, ConfigData>();

            internal static bool Init()
            {
                string[] files = WurmPaths.GetGameSettingsFilesPaths();
                if (files != null)
                {
                    foreach (ConfigData config in AllConfigs.Values)
                    {
                        config.Dispose();
                    }
                    AllConfigs.Clear();
                    foreach (string filepath in files)
                    {
                        try
                        {
                            AllConfigs.Add(
                                GeneralHelper.GetLastDirNamefromDirPath(Path.GetDirectoryName(filepath)),
                                new ConfigData(filepath));
                        }
                        catch (Exception _e)
                        {
                            Logger.LogError("Error while processing config: " + filepath, THIS, _e);
                            //do not return false, some dirs may be empty
                        }
                    }
                    if (AllConfigs.Count > 0) return true;
                    else return false;
                }
                else return false;
            }

            /// <summary>
            /// returns config object for this name or null if doesn't exist
            /// </summary>
            /// <param name="configName"></param>
            /// <returns></returns>
            internal static ConfigData GetConfig(string configName)
            {
                try
                {
                    return AllConfigs[configName];
                }
                catch (Exception _e)
                {
                    Logger.LogDebug("Failed to retrieve config: " + configName, THIS, _e);
                    return null;
                }
            }

            public static ConfigData[] GetAllConfigs()
            {
                return AllConfigs.Values.ToArray();
            }
        }

        public static class Autoexecs
        {
            private const string THIS = "WurmClient::Autoexecs";

            public class AutoexecManager
            {
                private readonly string _player;
                private string _autoexecFilePath;
                private readonly bool _isInvalid = false;

                public string Player { get { return _player; } }

                public AutoexecManager(string playerName)
                {
                    _player = playerName;
                    Configs.ConfigData configData = PlayerConfigurations.GetThisPlayerConfig(playerName);
                    if (configData == null)
                    {
                        _isInvalid = true;
                        throw new InvalidOperationException(
                            string.Format(
                                "Config for player {0} does not exist or there was an error when attempting to retrieve it.",
                                playerName)
                            );
                    }

                    if (configData.ExecSource == Configs.EnumFileSource.PlayerFolder)
                    {
                        _autoexecFilePath = Path.Combine(WurmPaths.GetPlayerDir(_player), "autorun.txt");
                    }
                    else if (configData.ExecSource == Configs.EnumFileSource.ProfileFolder)
                    {
                        _autoexecFilePath = Path.Combine(configData.ConfigDir, "autorun.txt");
                    }
                }

                /// <summary>
                /// Appends a command to autoexec.txt, throws exceptions on errors
                /// </summary>
                /// <exception cref="InvalidOperationException"></exception>
                /// <param name="execCommand"></param>
                public void AppendIfNotExists(string execCommand)
                {
                    if (!_isInvalid)
                    {
                        bool exists = false;
                        //read the exec file
                        using (var sr = new StreamReader(_autoexecFilePath))
                        {
                            //try to find the command text on each line
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                if (!line.StartsWith("//", StringComparison.InvariantCulture) && line.Contains(execCommand))
                                {
                                    exists = true;
                                    break;
                                }
                            }
                        }

                        //if not exists, append as new line
                        if (!exists)
                        {
                            using (var sw = new StreamWriter(_autoexecFilePath, true))
                            {
                                sw.WriteLine(); //this ensures command is always written on new line
                                sw.WriteLine(execCommand);
                            }
                        }
                    }
                    else throw new InvalidOperationException("This manager is not in valid state");
                }
            }

            public static bool Init()
            {
                try
                {
                    //default stuff needed to be in autoexecs at all times
                    AppendIfNotExistToAll("say /uptime");
                    AppendIfNotExistToAll("say /time");
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogError("problem initializing", THIS, ex);
                    return false;
                }
            }

            /// <summary>
            /// Gets manager for this player autoexec.txt file. Throws exception on error.
            /// </summary>
            /// <param name="playerName"></param>
            /// <exception cref="InvalidOperationException"></exception>
            /// <returns></returns>
            public static AutoexecManager GetAutoexecManagerForPlayer(string playerName)
            {
                return new AutoexecManager(playerName);
            }

            public static void AppendIfNotExistToAll(string execCommand)
            {
                var names = WurmPaths.GetAllPlayersNames();
                foreach (var name in names)
                {
                    try
                    {
                        var man = GetAutoexecManagerForPlayer(name);
                        man.AppendIfNotExists(execCommand);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogInfo("Updating autoexec failed for " + name, THIS, ex);
                    }
                }
            }
        }

        /// <summary>
        /// Provides means to interact with specific player profile
        /// </summary>
        public static class PlayerConfigurations
        {
            const string THIS = "WurmClient::PlayerConfigurations";

            static PlayerConfigurations()
            {
                WurmClient.InitIfNotInited();
            }

            static Dictionary<string, Configs.ConfigData> PlayerToConfigMap = new Dictionary<string, Configs.ConfigData>();

            static FileSystemWatcher changeWatcher;

            internal static bool Init()
            {
                if (changeWatcher != null) changeWatcher.Dispose();
                PlayerToConfigMap.Clear();
                try
                {
                    changeWatcher = new FileSystemWatcher(WurmPaths.PlayersDir);
                    changeWatcher.Filter = "config.txt";
                    changeWatcher.Changed += new FileSystemEventHandler(OnFSWevent);
                    changeWatcher.EnableRaisingEvents = true;

                    string[] allPlayerDirs = Directory.GetDirectories(WurmPaths.PlayersDir);
                    //create config objects and assign them to players
                    foreach (var dir in allPlayerDirs)
                    {
                        string playerName = GeneralHelper.GetLastDirNamefromDirPath(dir);
                        string configDefinitionFilePath = Path.Combine(dir, "config.txt");
                        string configName = ReadConfigNameFromFile(configDefinitionFilePath);
                        Configs.ConfigData config = Configs.GetConfig(configName);
                        PlayerToConfigMap.Add(playerName, config);
                    }
                    if (PlayerToConfigMap.Count > 0) return true;
                    else return false;
                }
                catch (Exception _e)
                {
                    Logger.LogError("Error while setting up the player-config map", THIS, _e);
                    return false;
                }
            }

            static string ReadConfigNameFromFile(string filepath)
            {
                if (File.Exists(filepath))
                {
                    string text;
                    try
                    {
                        using (StreamReader sr = new StreamReader(filepath))
                        {
                            text = sr.ReadToEnd().Trim();
                        }
                    }
                    catch (Exception _e)
                    {
                        Logger.LogError("", THIS, _e);
                        return null;
                    }
                    return text;
                }
                else
                {
                    Logger.LogDebug("File did not exist: " + filepath, THIS);
                    return null;
                }
            }

            static object FSWevent_lock = new object();
            static void OnFSWevent(object sender, FileSystemEventArgs e)
            {
                lock (FSWevent_lock)
                {
                    string filepath = e.FullPath;
                    string maybeNewConfigName = ReadConfigNameFromFile(filepath);
                    string playerName = GeneralHelper.GetLastDirNamefromDirPath(Path.GetDirectoryName(filepath));
                    if (PlayerToConfigMap[playerName].ConfigName != maybeNewConfigName)
                    {
                        PlayerToConfigMap[playerName] = Configs.GetConfig(maybeNewConfigName);
                    }
                }
            }

            /// <summary>
            /// returns config object for this player or null if not available
            /// </summary>
            /// <param name="playerName"></param>
            /// <returns></returns>
            public static Configs.ConfigData GetThisPlayerConfig(string playerName)
            {
                try
                {
                    return PlayerToConfigMap[playerName];
                }
                catch (Exception _e)
                {
                    Logger.LogError("", THIS, _e);
                    return null;
                }
            }

            //to implement when needed

            //public static [keybindingsObject] GetKeybindings(string playerName)
            //{
            //}

            //public static [timersObject] GetTimers(string playerName)
            //{
            //}

            //public static [screenshotsObject] GetScreenshots(string playerName)
            //{
            //}

            //public static [dumpsObject] GetSkillDumps(string playerName)
            //{
            //}

            //public static [playerdataObject] GetPlayerData(string playerName)
            //{
            //}

            //public static [windowsObject[]] GetWindowsLayoutData(string playerName)
            //{
            //}
        }
    }
}
