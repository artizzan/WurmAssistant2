﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Aldurcraft.Spellbook40.Io;

namespace Aldurcraft.Spellbook40.SimpleLogger
{
    public class SimpleLogger
    {
        public SimpleLogger()
        {
            ConsoleDefaultOut = Console.Out;
            ConsoleDefaultErrorOut = Console.Error;

            ErrorCount = 0;
            CriticalErrorCount = 0;
            LogInfo("Logging started " + DateTime.Now.ToString());
        }


        const string UNKNOWN_LOG_SOURCE = "-";
        int MAX_ALLOWED_MEGABYTES_FOR_LOGS = 4;
        const string LOGGING_STARTED = "LOGGER: Logging started";

        TextWriter ConsoleDefaultOut;
        TextWriter ConsoleDefaultErrorOut;

        TextBox tbOutput = null;
        LogMessagePriority priorityTreshhold = LogMessagePriority.Info;
        int PruneAT = 500;
        int PruneQuantityToKeep = 50;

        /// <summary>
        /// Number of errors (LogError + LogCritical) since current session started
        /// </summary>
        public int ErrorCount { get; private set; }
        /// <summary>
        /// Number of critical errors (LogCritical) since current session started
        /// </summary>
        public int CriticalErrorCount { get; private set; }

        int lastDayLogged = 0;
        /// <summary>
        /// Current directory where logs are being saved
        /// </summary>
        public string LogSaveDir { get; private set; }
        string _logSavePath;

        /// <summary>
        /// Will cache any messages that happen before textbox output is assigned, display them afterwards
        /// </summary>
        public bool CacheForTBOut { get; set; }
        Queue<LogMessage> CachedForTBOut = new Queue<LogMessage>();

        /// <summary>
        /// Get full path to the currently written log file
        /// </summary>
        public string LogSavePath
        {
            get
            {
                if (LogSaveDir == null) return null;
                if (DateTime.Now.Day != lastDayLogged)
                {
                    _logSavePath = Path.Combine(LogSaveDir, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
                    lastDayLogged = DateTime.Now.Day;
                }
                return _logSavePath;
            }
        }

        void TryCleanUpOldLogs()
        {
            try
            {
                string[] files = Directory.GetFiles(LogSaveDir);

                long totalBytes = 0;
                foreach (string file in files)
                {
                    FileInfo info = new FileInfo(file);
                    totalBytes += info.Length;
                }

                if (totalBytes > 1024 * 1024 * MAX_ALLOWED_MEGABYTES_FOR_LOGS) //max allowed for logs is 64 meg
                {
                    foreach (var file in files)
                    {
                        Match match = Regex.Match(Path.GetFileNameWithoutExtension(file), @"(\d\d\d\d)-(\d\d)-(\d\d)");
                        if (match.Success)
                        {
                            DateTime dt = new DateTime(
                                Convert.ToInt32(match.Groups[1].Value),
                                Convert.ToInt32(match.Groups[2].Value),
                                Convert.ToInt32(match.Groups[3].Value));
                            if (dt < DateTime.Now - TimeSpan.FromDays(60))
                                File.Delete(file);
                        }
                        else
                        {
                            LogError("found non-parseable log file name or non-log file in log dir, logs need a dedicated directory! Current dir: " + LogSaveDir);
                        }
                    }
                }
            }
            catch (Exception _e)
            {
                LogError("error cleaning old log files", "Logger", _e);
            }
        }

        StringWriterFlushEvent ConsoleOutStringWriter = null;
        ConsoleHandlingOption ConsoleHandlingMode = ConsoleHandlingOption.None;
        /// <summary>
        /// Allows to send console outputs to logger or vice versa
        /// </summary>
        /// <param name="option"></param>
        public void SetConsoleHandlingMode(ConsoleHandlingOption option)
        {
            try
            {
                ConsoleHandlingMode = option;
                if (option == ConsoleHandlingOption.SendConsoleToLoggerOutputDEBUG
                    || option == ConsoleHandlingOption.SendConsoleToLoggerOutputDIAG
                    || option == ConsoleHandlingOption.SendConsoleToLoggerOutputINFO)
                {
                    if (ConsoleOutStringWriter == null)
                    {
                        ConsoleOutStringWriter = new StringWriterFlushEvent(true);
                        Console.SetOut(ConsoleOutStringWriter);
                        Console.SetError(ConsoleOutStringWriter);
                    }
                    ConsoleOutStringWriter.Flushed += ConsoleOutStringWriter_Flushed;
                }
                else
                {
                    if (ConsoleOutStringWriter != null)
                    {
                        ConsoleOutStringWriter.Flushed -= ConsoleOutStringWriter_Flushed;
                        ConsoleOutStringWriter.Dispose();
                        ConsoleOutStringWriter = null;
                        Console.SetOut(ConsoleDefaultOut);
                        Console.SetError(ConsoleDefaultErrorOut);
                    }
                }
            }
            catch (Exception _e)
            {
                ConsoleHandlingMode = ConsoleHandlingOption.None;
                LogCritical("exception while attempting to set console handling mode", "Logger", _e);
                Console.SetOut(ConsoleDefaultOut);
                Console.SetError(ConsoleDefaultErrorOut);
            }
        }

        void ConsoleOutStringWriter_Flushed(object sender, StringWriterFlushEventArgs e)
        {
            switch (ConsoleHandlingMode)
            {
                case ConsoleHandlingOption.SendConsoleToLoggerOutputDEBUG:
                    LogDebug(e.Value, "[CONSOLE]");
                    break;
                case ConsoleHandlingOption.SendConsoleToLoggerOutputDIAG:
                    LogDiag(e.Value, "[CONSOLE]");
                    break;
                case ConsoleHandlingOption.SendConsoleToLoggerOutputINFO:
                    LogInfo(e.Value, "[CONSOLE]");
                    break;
            }
        }

        /// <summary>
        /// Set a winforms TextBox as an output target. Updated in thread safe way. Set null to disable. Default null.
        /// </summary>
        /// <param name="tb">Should be set for multiline, will only update the text</param>
        /// <param name="priorityTresh">(optional) Priority treshhold at which message should be forwarded to TextBox</param>
        /// <param name="pruneAt">(optional) Will remove oldest entries once number of lines in TextBox hits this number, default 500</param>
        /// <param name="pruneQuantity">(optional) Will prune this amount of oldest entries form TextBox, default 50</param>
        public void SetTBOutput(TextBox tb, LogMessagePriority priorityTresh = LogMessagePriority.Info, int pruneAt = 500, int pruneQuantity = 50)
        {
#if DEBUG
            priorityTresh = LogMessagePriority.Debug;
#endif
            tbOutput = tb;
            priorityTreshhold = priorityTresh;
            if (pruneQuantity > pruneAt) pruneQuantity = pruneAt - 1;
            PruneAT = pruneAt;
            PruneQuantityToKeep = pruneAt - pruneQuantity;

            if (CacheForTBOut)
            {
                CacheForTBOut = false;
                while (CachedForTBOut.Count > 0)
                {
                    SendMessageToTextBoxOutput(CachedForTBOut.Dequeue());
                }
            }
        }

        /// <summary>
        /// Sets a directory for logging output. Creates directory if not exist.
        /// </summary>
        /// <param name="directoryPath">Just directory path, without file name</param>
        /// <returns></returns>
        public bool SetLogSaveDir(string directoryPath)
        {
            if (directoryPath == null)
            {
                LogInfo("log save path was null", "Logger");
                return false;
            }

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            LogSaveDir = directoryPath;

            TryCleanUpOldLogs();

            LogDiag(LOGGING_STARTED);

            return true;
        }

        internal void WriteText(string message, string category, LogMessagePriority priority)
        {
            try
            {
                UpdateErrorCounts(priority);
                string cat = category;
                LogMessage logmessage = new LogMessage(DateTimeOffset.Now, cat, priority, message);

                ForwardLogMessage(logmessage, category);
            }
            catch (Exception _e)
            {
                Debug.WriteLine("LOGGER ERROR!");
                Debug.WriteLine(_e.Message);
                Debug.WriteLine(_e.Source);
                Debug.WriteLine(_e.StackTrace);
            }
        }

        internal void WriteException(string message, string category, LogMessagePriority priority, Exception e, bool warningsound = false)
        {
            try
            {
                UpdateErrorCounts(priority);

                if (warningsound) System.Media.SystemSounds.Hand.Play();

                string mess = "";
                if (!string.IsNullOrEmpty(message)) mess += message + "\r\n";
                mess += "EXCEPTIONS: ";
                var sb = new StringBuilder();
                WriteExceptions(sb, e);

                string cat = category;

                LogMessage logmessage = new LogMessage(DateTimeOffset.Now, cat, priority, mess, sb.ToString());

                ForwardLogMessage(logmessage, category);
            }
            catch (Exception _e)
            {
                Debug.WriteLine("LOGGER ERROR!");
                Debug.WriteLine(_e.Message);
                Debug.WriteLine(_e.Source);
                Debug.WriteLine(_e.StackTrace);
            }
        }

        private void WriteExceptions(StringBuilder sb, Exception e)
        {
            sb.Append(FormattedException(e));

            if (e is AggregateException)
            {
                var agExc = (AggregateException) e;
                foreach (var innerException in agExc.InnerExceptions)
                {
                    WriteExceptions(sb, innerException);
                }
            }
            if (e.InnerException != null)
            {
                WriteExceptions(sb, e.InnerException);
            }
        }

        private string FormattedException(Exception e)
        {
            return String.Format("\r\nMESSAGE: {3}\r\nTYPE: {0}\r\nSOURCE: {1}\r\nTRACE: {2}", 
                e.GetType(), e.Source, e.StackTrace, e.Message);
        }

        /// <summary>
        /// Fired when any error (LogError, LogCritical) is logged. 
        /// Note: this event will be fired on thread where Log was called.
        /// </summary>
        public event EventHandler ErrorLogged;

        void UpdateErrorCounts(LogMessagePriority priority)
        {
            switch (priority)
            {
                case LogMessagePriority.Error:
                    ErrorCount += 1;
                    var eh = ErrorLogged; //tsafe
                    if (eh != null)
                    {
                        try
                        {
                            eh("Logger", new EventArgs());
                        }
                        catch (Exception _e)
                        {
                            MessageBox.Show(_e.Message, "Logger::UpdateErrorCounts event failed", MessageBoxButtons.OK);
                        }
                    }
                    break;
                case LogMessagePriority.Critical:
                    ErrorCount += 1;
                    CriticalErrorCount += 1;
                    var eh2 = ErrorLogged; //tsafe
                    if (eh2 != null)
                    {
                        try
                        {
                            eh2("Logger", new EventArgs());
                        }
                        catch (Exception _e)
                        {
                            MessageBox.Show(_e.Message, "Logger::UpdateErrorCounts event failed", MessageBoxButtons.OK);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Returns list of LogMessagePriority enumeration as strings
        /// </summary>
        /// <returns></returns>
        public string[] GetPriorities()
        {
            return Enum.GetNames(typeof(LogMessagePriority));
        }

        //logging engine

        readonly object fileSaveLock = new object();
        void ForwardLogMessage(LogMessage logmessage, string category)
        {
            if (ConsoleHandlingMode == ConsoleHandlingOption.SendLoggerOutputToConsole)
            {
                string output = String.Format("[{0}] {1} > {2} > {3}{4}",
                    logmessage.Timestamp.ToString("HH:mm:ss"),
                    logmessage.GetPriorityString(),
                    logmessage.Category,
                    logmessage.Message,
                    logmessage.Details);

                Console.WriteLine(output);
            }

            if (ConsoleHandlingMode == ConsoleHandlingOption.SendLoggerOutputToTrace)
            {
                string output = String.Format("[{0}] {1} > {2} > {3}{4}",
                    logmessage.Timestamp.ToString("HH:mm:ss"),
                    logmessage.GetPriorityString(),
                    logmessage.Category,
                    logmessage.Message,
                    logmessage.Details);

                Trace.WriteLine(output);
            }

            if (LogSavePath != null)
            {
                lock (fileSaveLock)
                {
                    using (StreamWriter fs = new StreamWriter(LogSavePath, true))
                    {
                        string output = String.Format("[{0}] {1} > {2} > {3}{4}",
                            logmessage.Timestamp.ToString("HH:mm:ss"),
                            logmessage.GetPriorityString(),
                            logmessage.Category,
                            logmessage.Message,
                            logmessage.Details);

                        fs.WriteLine(output);
                    }
                }
            }

            if (CacheForTBOut)
            {
                CachedForTBOut.Enqueue(logmessage);
            }

            if (tbOutput != null)
            {
                SendMessageToTextBoxOutput(logmessage);
            }
        }

        void SendMessageToTextBoxOutput(LogMessage logmessage)
        {
            if (logmessage.Priority >= priorityTreshhold)
            {
                try
                {
                    tbOutput.BeginInvoke(new Action<LogMessage>(UpdateTextBoxOutput), new object[] { logmessage });
                }
                catch (Exception _e)
                {
                    tbOutput = null;
                    LogError("failed to update textbox output, disabling", "Logger", _e);
                }
            }
        }

        void UpdateTextBoxOutput(LogMessage message)
        {
            try
            {
#if DEBUG
                message.Message = message.Message + message.Details;
#endif
                List<string> contents = new List<string>();
                contents.AddRange(tbOutput.Lines);
                contents.Add(String.Format("[{0}] {1} > {2} > {3}",
                            message.Timestamp.ToString("HH:mm:ss"),
                            message.GetPriorityString(),
                            message.Category,
                            message.Message));
                if (contents.Count <= PruneAT)
                {
                    tbOutput.Lines = contents.ToArray();
                }
                else
                {
                    tbOutput.Lines = contents.GetRange(contents.Count - PruneQuantityToKeep, PruneQuantityToKeep).ToArray();
                }
            }
            catch (Exception _e)
            {
                tbOutput = null;
                LogError("failed to update textbox output within invoke method", "Logger", _e);
            }
        }

        public void OpenLogDir()
        {
            try
            {
                Process.Start(LogSaveDir);
            }
            catch (Exception _e)
            {
                LogError("Could not open my own log files dir! How rude!", "Logger", _e);
            }
        }

        /// <summary>
        /// Intended for debugging messages. Conditional DEBUG, does not compile for release builds.
        /// </summary>
        /// <param name="message">Message to be logged. Exception data is logged automatically.</param>
        /// <param name="source">Source object or a string describing source.</param>
        /// <param name="_e">Exception object, if applicable (logs message and trace)</param>
        [Conditional("DEBUG")]
        public void LogDebug(string message, object source = null, Exception _e = null)
        {
            if (source == null) source = UNKNOWN_LOG_SOURCE;
            if (_e == null) WriteText(message, source.ToString(), LogMessagePriority.Debug);
            else WriteException(message, source.ToString(), LogMessagePriority.Debug, _e);
        }

        /// <summary>
        /// Intended for diagnostic messages. By default will be output only to text file.
        /// Use to trace program execution in release builds without flooding user log view.
        /// </summary>
        /// <param name="message">Message to be logged. Exception data is logged automatically.</param>
        /// <param name="source">Source object or a string describing source.</param>
        /// <param name="_e">Exception object, if applicable (logs message and trace)</param>
        public void LogDiag(string message, object source = null, Exception _e = null)
        {
            if (source == null) source = UNKNOWN_LOG_SOURCE;
            if (_e == null) WriteText(message, source.ToString(), LogMessagePriority.Diag);
            else WriteException(message, source.ToString(), LogMessagePriority.Diag, _e);
        }

        /// <summary>
        /// Intended for informational and flow messages.
        /// Use to indicate key execution events and state changes to the users, if needed.
        /// </summary>
        /// <param name="message">Message to be logged. Exception data is logged automatically.</param>
        /// <param name="source">Source object or a string describing source.</param>
        /// <param name="_e">Exception object, if applicable (logs message and trace)</param>
        public void LogInfo(string message, object source = null, Exception _e = null)
        {
            if (source == null) source = UNKNOWN_LOG_SOURCE;
            if (_e == null) WriteText(message, source.ToString(), LogMessagePriority.Info);
            else WriteException(message, source.ToString(), LogMessagePriority.Info, _e);
        }

        /// <summary>
        /// Intended for general error messages. Use to indicate an unexpected error,
        /// that is not critical for continued running of the program.
        /// </summary>
        /// <param name="message">Message to be logged. Exception data is logged automatically.</param>
        /// <param name="source">Source object or a string describing source.</param>
        /// <param name="_e">Exception object, if applicable (logs message and trace)</param>
        public void LogError(string message, object source = null, Exception _e = null)
        {
            if (source == null) source = UNKNOWN_LOG_SOURCE;
            if (_e == null) WriteText(message, source.ToString(), LogMessagePriority.Error);
            else WriteException(message, source.ToString(), LogMessagePriority.Error, _e);
        }

        /// <summary>
        /// Intended for critical error messages. Use to indicate an unexpected error,
        /// that can possibly cause further issues or data corruption within a program.
        /// It is recommended to terminate program or unit after logging such error.
        /// </summary>
        /// <param name="message">Message to be logged. Exception data is logged automatically.</param>
        /// <param name="source">Source object or a string describing source.</param>
        /// <param name="_e">Exception object, if applicable (logs message and trace)</param>
        public void LogCritical(string message, object source = null, Exception _e = null)
        {
            if (source == null) source = UNKNOWN_LOG_SOURCE;
            if (_e == null) WriteText(message, source.ToString(), LogMessagePriority.Critical);
            else WriteException(message, source.ToString(), LogMessagePriority.Critical, _e);
        }

        [EditorBrowsable(EditorBrowsableState.Never)][Obsolete]
        public void Log(LogMessagePriority priority, string message, string category = UNKNOWN_LOG_SOURCE, Exception _e = null)
        {
            if (_e == null) WriteText(message, category, priority);
            else WriteException(message, category, priority, _e);
        }

        [EditorBrowsable(EditorBrowsableState.Never)][Obsolete]
        public void __WriteLine(string note)
        {
            WriteText(note, UNKNOWN_LOG_SOURCE, LogMessagePriority.Info);
        }

        [EditorBrowsable(EditorBrowsableState.Never)][Obsolete]
        void __LogException(Exception _e, bool warningsound = false)
        {
            WriteException(null, UNKNOWN_LOG_SOURCE, LogMessagePriority.Error, _e, warningsound: warningsound);
        }


        public enum LogMessagePriority { Debug, Diag, Info, Error, Critical }
        public enum ConsoleHandlingOption { 
            None, 
            SendLoggerOutputToConsole, 
            SendConsoleToLoggerOutputDEBUG, 
            SendConsoleToLoggerOutputDIAG, 
            SendConsoleToLoggerOutputINFO,
            SendLoggerOutputToTrace
        }

        struct LogMessage
        {
            public readonly DateTimeOffset Timestamp;
            public readonly string Category;
            public readonly LogMessagePriority Priority;
            public string Message;
            public string Details;

            public LogMessage(
                DateTimeOffset timestamp, string category, LogMessagePriority priority,
                string message, string details = null)
            {
                Timestamp = timestamp;
                Category = category;
                Priority = priority;
                Message = message;
                Details = (details ?? "");
            }

            public string GetPriorityString()
            {
                switch (Priority)
                {
                    case LogMessagePriority.Critical:
                        return "CRITICAL ERROR";
                    case LogMessagePriority.Error:
                        return "ERROR";
                    case LogMessagePriority.Info:
                        return "Info";
                    case LogMessagePriority.Diag:
                        return "Diag";
                    case LogMessagePriority.Debug:
                        return "Debug";
                    default:
                        return "Default";
                }
            }
        }
    }
}
