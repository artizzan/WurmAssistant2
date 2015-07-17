using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using Aldurcraft.WurmOnline.WurmLogsManager;
using Aldurcraft.WurmOnline.WurmState;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmLogsManager
{
    using SysTimer = System.Timers.Timer;

    /// <summary>
    /// Abstracts wurm client log files, provides means to obtain latest events "on the fly" as they happen in-game.
    /// </summary>
    public static class WurmLogs
    {
        const string THIS = "WurmLogs";

        static Dictionary<string, LogEngine> LogEngines = new Dictionary<string, LogEngine>();
        static SysTimer UpdateLoop;

        static WurmLogs()
        {
            UpdateLoop = new SysTimer();
            UpdateLoop.Enabled = false;
            UpdateLoop.Interval = UpdateInterval;
            UpdateLoop.AutoReset = false;
            UpdateLoop.Elapsed += UpdateLoop_Elapsed;
        }

        static void UpdateLoop_Elapsed(object sender, EventArgs e)
        {
            foreach (var engine in LogEngines)
            {
                engine.Value.UpdateAndBroadcastNewEvents();
            }

            UpdateLoop.Enabled = true; //allow running again only once this finishes
        }

        static int _UpdateInterval = 500;
        /// <summary>
        /// Gets or sets interval, at which wurm game logs are checked for changes, default 500 [milliseconds].
        /// </summary>
        public static int UpdateInterval
        {
            get { return _UpdateInterval; }
            set { _UpdateInterval = value; }
        }

        /// <summary>
        /// Set existing System.Windows.Forms.Control as synchronizing object for this assembly internals. 
        /// Setting this makes it unnecessary to marshal events coming out of this assembly, provided
        /// that handling code is also using same synchronizing context.
        /// </summary>
        /// <param name="control"></param>
        public static void AssignSynchronizingObject(Control control)
        {
            UpdateLoop.SynchronizingObject = control;
        }

        /// <summary>
        /// Enable reading wurm game logs and broadcasting events
        /// </summary>
        public static void Enable()
        {
            UpdateLoop.Enabled = true;
        }

        /// <summary>
        /// Disable reading wurm game logs and broadcasting events
        /// </summary>
        public static void Disable()
        {
            UpdateLoop.Enabled = false;
        }

        /// <summary>
        /// Creates a new wurm-log reading worker for specified wurm character. 
        /// Returns true if succeeded, will fail if worker already running for this wurm character,
        /// or there is no path/config data available to initialize it.
        /// Note: subscribing and unsubscribing to this API automatically handles this process!
        /// </summary>
        /// <param name="playerName">full wurm character name, case-sensitive</param>
        /// <param name="dailyLoggingMode">optional, recommended null, allows overriding autodetected logging mode;
        /// true = daily logging mode; false = monthly logging mode</param>
        /// <returns></returns>
        public static bool CreateEngine(string playerName, bool? dailyLoggingMode = null)
        {
            try
            {
                if (dailyLoggingMode == null)
                {
                    WurmClient.Configs.ConfigData config = WurmClient.PlayerConfigurations.GetThisPlayerConfig(playerName);
                    WurmClient.Configs.EnumLoggingType[] allowedTypes = 
                    { 
                        WurmClient.Configs.EnumLoggingType.Daily,
                        WurmClient.Configs.EnumLoggingType.Monthly 
                    };
                    if (config.EventAndOtherLoggingModesAreEqual(allowedTypes))
                    {
                        if (config.EventLoggingType == WurmClient.Configs.EnumLoggingType.Daily) dailyLoggingMode = true;
                        else if (config.EventLoggingType == WurmClient.Configs.EnumLoggingType.Monthly) dailyLoggingMode = false;
                        else
                        {
                            Logger.LogError("Unknown logging mode for this character: " + (playerName ?? "NULL"), THIS);
                            return false;
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("Wurm client logging modes are set incorrectly: Event: {0}, Other: {1}",
                            config.EventLoggingType, config.OtherLoggingType));
                    }
                }
                LogEngine newEngine = new LogEngine(playerName, dailyLoggingMode.Value);
                LogEngines.Add(playerName, newEngine);
                return true;
            }
            catch (Exception _e)
            {
                Logger.LogError("Could not create log engine for " + (playerName ?? "NULL") + " ; daily logging: " + dailyLoggingMode.ToString(), THIS, _e);
                return false;
            }
        }

        /// <summary>
        /// Method intended for garbage collection tests
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="engine"></param>
        /// <param name="dailyLoggingMode"></param>
        /// <returns></returns>
        internal static bool CreateAndReturnEngine(string playerName, out LogEngine engine, bool? dailyLoggingMode = null)
        {
            try
            {
                if (dailyLoggingMode == null)
                {
                    WurmClient.Configs.ConfigData config = WurmClient.PlayerConfigurations.GetThisPlayerConfig(playerName);
                    if (config.EventLoggingType == config.OtherLoggingType)
                    {
                        if (config.EventLoggingType == WurmClient.Configs.EnumLoggingType.Daily) dailyLoggingMode = true;
                        else if (config.EventLoggingType == WurmClient.Configs.EnumLoggingType.Monthly) dailyLoggingMode = false;
                        else
                        {
                            engine = null;
                            Logger.LogError("Could not create engine because of unsupported logging mode: " + config.EventLoggingType.ToString(), THIS);
                            return false;
                        }
                    }
                }
                engine = new LogEngine(playerName, dailyLoggingMode.Value);
                LogEngines.Add(playerName, engine);
                Logger.LogInfo("Created engine for player " + playerName);
                return true;
            }
            catch (Exception _e)
            {
                Logger.LogError("Could not create log engine for " + playerName + " ; daily logging: " + dailyLoggingMode.ToString(), THIS, _e);
                engine = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to kill wurm-log reading worker for specified player. Clears event subscriptions automatically.
        /// Returns true if worker is successfully stopped or didn't exist.
        /// </summary>
        /// <param name="playerName">wurm character name, case sensitive</param>
        /// <returns></returns>
        public static bool KillEngine(string playerName)
        {
            try
            {
                LogEngines[playerName].Dispose(); //must clear event handlers
                LogEngines.Remove(playerName); //should be garbaged now
                return true;
            }
            catch (Exception _e)
            {
                if (!LogEngines.ContainsKey(playerName))
                {
                    Logger.LogDebug("Could not kill log engine, maybe didn't exist: " + (playerName ?? "NULL"), THIS, _e);
                    return true;
                }
                else
                {
                    Logger.LogError("Could not kill log engine: " + (playerName ?? "NULL"), THIS, _e);
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns names of all characters, that have active wurm-log worker.
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllLogEngines()
        {
            return LogEngines.Keys.ToArray();
        }

        /// <summary>
        /// Subscribes to wurm-log worker for specified wurm character. Will attempt to create engine if missing.
        /// Will return false on fail.
        /// </summary>
        /// <param name="playerName">wurm character name, case sensitive</param>
        /// <param name="callback">event handler for new log messages; 
        ///  note: without setting synchronizing object for this manager, 
        ///  handling may require additional synchronizing logic</param>
        /// <returns></returns>
        /// <remarks>
        /// 
        /// </remarks>
        public static bool SubscribeToLogFeed(string playerName, EventHandler<NewLogEntriesEventArgs> callback)
        {
            try
            {
                LogEngines[playerName].NewLogEntries += callback;
                Logger.LogInfo("Subscribed to engine for player " + playerName, THIS);
                return true;
            }
            catch (KeyNotFoundException)
            {
                try
                {
                    Logger.LogInfo("Engine missing, trying to create", THIS);
                    CreateEngine(playerName);
                    LogEngines[playerName].NewLogEntries += callback;
                    Logger.LogInfo("Subscribed to engine for player " + playerName, THIS);
                    return true;
                }
                catch (Exception _e)
                {
                    Logger.LogError("Could not subscribe to log engine: " + playerName, THIS, _e);
                    return false;
                }
            }
            catch (Exception _e)
            {
                Logger.LogError("Could not subscribe to log engine: " + playerName, THIS, _e);
                return false;
            }
        }

        /// <summary>
        /// Unsubscribes from wurm-log worker for specified wurm character. Returns false if any error.
        /// Returns true if there was no subscription present.
        /// </summary>
        /// <param name="playerName">Wurm character name, case sensitive</param>
        /// <param name="callback">Event handler used to handle this log feed</param>
        /// <returns></returns>
        public static bool UnsubscribeFromLogFeed(string playerName, EventHandler<NewLogEntriesEventArgs> callback)
        {
            try
            {
                LogEngines[playerName].NewLogEntries -= callback;
                Logger.LogInfo("Unsubscribed from engine for player " + playerName, THIS);
                return true;
            }
            catch (Exception _e)
            {
                Logger.LogError("Could not unsubscribe from log engine: " + playerName, THIS, _e);
                return false;
            }
        }

        public static bool CreateAndReturnEngine_Test(string text, out LogEngine engine)
        {
            return CreateAndReturnEngine(text, out engine);
        }
    }
}
