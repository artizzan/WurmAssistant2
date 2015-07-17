using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Windows.Forms;

namespace Aldurcraft.Utility
{
    /// <summary>
    /// This class is intended to wrap around Process, in particular console apps, 
    /// so that console and error output can be read and input sent to the app.
    /// </summary>
    /// <remarks>
    /// Requires disposing. Supports waiting for exit, killing process, thread safe input sending.
    /// </remarks>
    public class ProcessManager : IDisposable
    {
        public class OutEventArgs : EventArgs
        {
            public readonly string Text;
            public OutEventArgs(string text) { this.Text = text; }
        }

        public class ProcessStillRunningException : Exception
        {
            public ProcessStillRunningException(string message)
                : base(message)
            {
            }
        }

        Process WrappedProcess;
        System.Timers.Timer OutputReaderLoop;
        System.Timers.Timer ErrorReaderLoop;

        /// <summary>
        /// Creates new ProcessManager. Can be optionally provided with any Control to join its synchronization context.
        /// </summary>
        /// <param name="syncControl"></param>
        public ProcessManager(Control syncControl = null)
        {
            WrappedProcess = new Process();

            OutputReaderLoop = new System.Timers.Timer();
            OutputReaderLoop.Elapsed += OutputReaderLoop_Elapsed;
            OutputReaderLoop.AutoReset = false;
            OutputReaderLoop.Interval = 100;

            ErrorReaderLoop = new System.Timers.Timer();
            ErrorReaderLoop.Elapsed += ErrorReaderLoop_Elapsed;
            ErrorReaderLoop.AutoReset = false;
            ErrorReaderLoop.Interval = 100;

            if (syncControl != null)
            {
                OutputReaderLoop.SynchronizingObject = syncControl;
                ErrorReaderLoop.SynchronizingObject = syncControl;
            }
        }

        /// <summary>
        /// Starts the process and begins monitoring output/error streams. Exception on any errors.
        /// </summary>
        /// <param name="workingDir">Working directory for launched process, full path</param>
        /// <param name="runFile">File to run, full path</param>
        /// <param name="arguments">Arguments to run with (same format as req by ProcessStartInfo)</param>
        public void StartProcess(string workingDir, string runFile, string arguments)
        {
            WrappedProcess = new Process();
            ProcessStartInfo info = new ProcessStartInfo(runFile, arguments)
            {
                CreateNoWindow = !ShowWindow,
                UseShellExecute = false,
                WorkingDirectory = workingDir,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };
            WrappedProcess.StartInfo = info;

            WrappedProcess.OutputDataReceived += WrappedProcess_OutputDataReceived;
            WrappedProcess.ErrorDataReceived += WrappedProcess_ErrorDataReceived;

            WrappedProcess.Start();

            WrappedProcess.BeginOutputReadLine();
            WrappedProcess.BeginErrorReadLine();

            OutputReaderLoop.Start();
            ErrorReaderLoop.Start();

            Logger.LogInfo("Process started: " + WrappedProcess.ProcessName, this);
        }

        ConcurrentQueue<string> OutputQueue = new ConcurrentQueue<string>();
        /// <summary>
        /// Received new standard output text
        /// </summary>
        public event EventHandler<OutEventArgs> OnNewStandardOutput;

        void WrappedProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OutputQueue.Enqueue(e.Data);
        }

        void OutputReaderLoop_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            string line;
            while (OutputQueue.TryDequeue(out line))
            {
                if (OnNewStandardOutput != null) OnNewStandardOutput(this, new OutEventArgs(line));
            }
            //allow next event after this one is completed, timer is set to disable itself on elapsed
            OutputReaderLoop.Enabled = true; 
        }

        ConcurrentQueue<string> ErrorQueue = new ConcurrentQueue<string>();
        /// <summary>
        /// Received new error output text
        /// </summary>
        public event EventHandler<OutEventArgs> OnNewErrorOutput;

        void WrappedProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            ErrorQueue.Enqueue(e.Data);
        }


        void ErrorReaderLoop_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            string line;
            while (ErrorQueue.TryDequeue(out line))
            {
                if (OnNewErrorOutput != null) OnNewErrorOutput(this, new OutEventArgs(line));
            }
            //allow next event after this one is completed, timer is set to disable itself on elapsed
            ErrorReaderLoop.Enabled = true; 
        }

        object _locker = new object();

        /// <summary>
        /// SendClone command into the wrapped Process. Returns false if sending failed, reason is logged as Diag.
        /// </summary>
        /// <param name="command"></param>
        public bool SendInput(string command)
        {
            lock (_locker)
            {
                try
                {
                    WrappedProcess.StandardInput.WriteLine(command);
                    return true;
                }
                catch (Exception _e)
                {
                    Logger.LogDiag("input send failed", this, _e);
                    return false;
                }
            }
        }

        /// <summary>
        /// Cleans event handlers and disposes the process, IF process has exited, else throws exception.
        /// </summary>
        /// <exception cref="ProcessStillRunningException"></exception>
        public void Dispose()
        {
            //TODO is it a correct practice for dispose to throw exceptions?

            if (WrappedProcess == null) 
            {
                Logger.LogDiag("WrappedProcess was null, returning", this);
                OnNewStandardOutput = null;
                OnNewErrorOutput = null;
                return;
            }

            bool processExitFlag;
            try { processExitFlag = WrappedProcess.HasExited; }
            catch (InvalidOperationException _e)
            {
                processExitFlag = true;
                Logger.LogDiag("WrappedProcess ref not tied to a real process", this, _e);
            }

            if (processExitFlag == false)
            {
                Logger.LogDiag("Dispose failed because associated process was still running", this);
                throw new ProcessStillRunningException("Dispose failed because associated process is still running!");
            }

            try
            {
                WrappedProcess.CancelOutputRead();
                WrappedProcess.CancelErrorRead();
            }
            catch (InvalidOperationException)
            {
                //yes microsoft, thanks for telling me its already cancelled. with an error. crashing my program.
            }

            OnNewStandardOutput = null;
            OnNewErrorOutput = null;

            WrappedProcess.Dispose();
        }

        /// <summary>
        /// Blocks the thread awaiting wrapped Process termination. Does not (?) block if process already finished.
        /// </summary>
        /// <param name="timeoutMillis"></param>
        /// <returns></returns>
        public bool WaitForExit(int timeoutMillis)
        {
            if (WrappedProcess == null) return true;
            return WrappedProcess.WaitForExit(timeoutMillis);
        }

        //public bool HasExited { get { if (WrappedProcess == null) return true; else return WrappedProcess.HasExited; } }

        /// <summary>
        /// Should the wrapped Process console window be shown, default false. 
        /// Effective only before StartProcess is called.
        /// </summary>
        public bool ShowWindow { get; set; }

        /// <summary>
        /// Forcefully aborts the process, not recommended unless as a last resort.
        /// </summary>
        /// <returns></returns>
        public bool KillProcess()
        {
            WrappedProcess.Kill();
            return true;
        }
    }
}
