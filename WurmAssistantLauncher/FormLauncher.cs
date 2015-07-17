using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Forms.VisualStyles;
using Newtonsoft.Json;
using SevenZip;

namespace WurmAssistantLauncher
{
    public partial class FormLauncher : Form
    {
        private const string BasePath = "http://old.aldurcraft.com/";
        //private const string BasePath = "http://aldurcraft.com/";
        //private const string BasePath = "http://localhost:19296/";
        //private const string BasePath = "http://localhost:19298/";
        private const string ControllerPath = "api/WurmAssistantApi";
        private const string RequestTemplate = "?id={0}";

        public FormLauncher()
        {
            InitializeComponent();
            Logger.SetLogSaveDir(GeneralHelper.PathCombineWithCodeBasePath("LauncherLogs"));
            Logger.SetConsoleHandlingMode(Logger.ConsoleHandlingOption.SendConsoleToLoggerOutputDIAG);
            button1.Text = "Run previous version";
            button1.Click += (sender, args) => this.BeginInvoke((Action)(RunLatestVersion));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            Task task = new Task(DoWork);
            task.Start(TaskScheduler.Default);
        }

        void DoWork()
        {
            //This code is fine. Fine I say.
            try
            {
                //find latest installed version
                SetStatus("Checking latest local version");
                SetDesc("");

                bool previousVersionAvailable = false;
                bool failed = false;

                var workdir = GeneralHelper.PathCombineWithCodeBasePath(null);
                var dirInfo = new DirectoryInfo(workdir);
                var existingWaDirs = dirInfo.GetDirectories();
                Version localLatestVersion = null;
                foreach (var dir in existingWaDirs)
                {
                    Match match = Regex.Match(dir.Name, @"WurmAssistant_(\d+)_(\d+)_(\d+)_(\d+)", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        var thisVersion = new Version(
                            int.Parse(match.Groups[1].Value),
                            int.Parse(match.Groups[2].Value),
                            int.Parse(match.Groups[3].Value),
                            int.Parse(match.Groups[4].Value));
                        if (localLatestVersion == null || thisVersion > localLatestVersion)
                        {
                            localLatestVersion = thisVersion;
                        }
                    }
                }

                if (localLatestVersion == null)
                {
                    SetDesc("No installed Wurm Assistant found, attempting to download latest...");
                }
                else
                {
                    previousVersionAvailable = true;
                }

                //check for new assistant version, timeout should be ~20 sec
                SetStatus("Checking for new Wurm Assistant version");

                const int maxRetries = 3;
                int retries = maxRetries;
                HttpResponseMessage response = null;

                while (retries != 0)
                {
                    var client = new HttpClient
                    {
                        BaseAddress = new Uri(BasePath),
                        Timeout = GetTimeout(maxRetries - retries + 1)
                    };
                    try
                    {
                        response = client.GetAsync(ControllerPath).Result;
                        if (!response.IsSuccessStatusCode)
                        {
                            retries--;
                            SetDesc("Failed contacting remote server: " + response.StatusCode + " => " +
                                    response.ReasonPhrase);
                            AllowRunningPrevious();
                        }
                        else
                        {
                            AllowRunningPrevious(false);
                            break;
                        }
                    }
                    catch (AggregateException agExc)
                    {
                        retries--;
                        string error = "Failed contacting remote server: " + string.Join(", ", agExc.InnerExceptions.Select(x => x.Message));
                        SetDesc(error);
                        LogException(agExc);
                        AllowRunningPrevious();
                    }

                    if (retries != 0)
                    {
                        SetDesc(string.Format("Retrying... ({0} of {1})", maxRetries - retries, maxRetries - 1), true);
                    }
                }

                if (response == null)
                {
                    SetFailure("Update failed", allowPreviousVersion: previousVersionAvailable);
                    return;
                }

                var obj = response.Content.ReadAsStringAsync().Result;
                var array = JsonConvert.DeserializeObject<string[]>(obj);

                //var array = new[] {"WurmAssistant_2_0_81_0.zip"};

                if (array.Length == 0)
                {
                    SetFailure("Update failed, no WA version on download server.", allowPreviousVersion: true);
                    return;
                }

                var remoteLatestVersion = new Version();
                string remoteFileName = null;
                foreach (var fileString in array)
                {
                    Match match = Regex.Match(fileString, @"WurmAssistant_(\d+)_(\d+)_(\d+)_(\d+)", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        var thisVersion = new Version(
                            int.Parse(match.Groups[1].Value),
                            int.Parse(match.Groups[2].Value),
                            int.Parse(match.Groups[3].Value),
                            int.Parse(match.Groups[4].Value));
                        if (thisVersion > remoteLatestVersion)
                        {
                            remoteLatestVersion = thisVersion;
                            remoteFileName = fileString;
                        }
                    }
                }

                if (localLatestVersion == null || remoteLatestVersion > localLatestVersion)
                {
                    //update
                    SetStatus("Downloading new Wurm Assistant version (" + remoteLatestVersion + ")");
                    SetProgressBarToBlocks();

                    const int maxTries = 3;
                    int currentTry = 1;
                    string downloadPath = GeneralHelper.PathCombineWithCodeBasePath(remoteFileName);

                    while (currentTry <= maxTries)
                    {
                        try
                        {
                            using (var webclient = new WebDownload((int)(GetTimeout(currentTry).TotalMilliseconds)))
                            {
                                var tcs = new TaskCompletionSource<bool>();
                                webclient.DownloadProgressChanged += (sender, args) =>
                                    SetProgressBar(args.BytesReceived, args.TotalBytesToReceive);
                                webclient.DownloadFileCompleted += (sender, args) =>
                                    tcs.SetResult(true);
                                webclient.DownloadFileAsync(
                                    new Uri(BasePath + ControllerPath + string.Format(RequestTemplate, remoteFileName)),
                                    downloadPath);
                                tcs.Task.Wait();
                                AllowRunningPrevious(false);
                            }
                            break;
                        }
                        catch (Exception ex)
                        {
                            SetDesc("Error while downloading new version: " + ex.Message);
                            currentTry++;
                            if (currentTry <= maxTries)
                            {
                                SetDesc("\r\n" + string.Format("Retrying... ({0} of {1})", currentTry - 1, maxTries - 1), true);
                                AllowRunningPrevious();
                            }
                            else
                            {
                                SetFailure("Download failed", allowPreviousVersion: previousVersionAvailable);
                                failed = true;
                                var fileinfo = new FileInfo(downloadPath);
                                if (fileinfo.Exists)
                                {
                                    try
                                    {
                                        fileinfo.Delete();
                                    }
                                    catch (Exception ex2)
                                    {
                                        SetDesc("failed to clean the partially downloaded file: " + downloadPath +
                                                "\r\n" + ex2.Message);
                                        LogException(ex2);
                                    }
                                }
                                break;
                            }
                        }
                    }
                    //extract
                    if (!failed)
                    {
                        SetStatus("Extracting new Wurm Assistant version");
                        try
                        {

                            var tempExtractDir = new DirectoryInfo(GeneralHelper.PathCombineWithCodeBasePath("temp"));

                            //clean up any past failed updates
                            if (tempExtractDir.Exists)
                            {
                                Thread.Sleep(100);
                                tempExtractDir.Delete(true);
                            }
                            Thread.Sleep(200);
                            tempExtractDir.Create();
                            Thread.Sleep(200);
                            using (var extractor = new SevenZipExtractor(downloadPath))
                            {
                                extractor.ExtractArchive(tempExtractDir.FullName);
                            }


                            var waDirName = Path.GetFileNameWithoutExtension(downloadPath);
                            var newAssistantDir = new DirectoryInfo(Path.Combine(tempExtractDir.FullName, waDirName));
                            var destinationPath = GeneralHelper.PathCombineWithCodeBasePath(waDirName);
                            const int maxMoveTries = 100;
                            int currentMoveTry = 0;
                            Thread.Sleep(500);
                            while (true)
                            {
                                currentMoveTry++;
                                try
                                {
                                    newAssistantDir.MoveTo(destinationPath);
                                    AllowRunningPrevious(false);
                                    break;
                                }
                                catch (IOException)
                                {
                                    if (currentMoveTry > maxMoveTries)
                                    {
                                        throw;
                                    }
                                    else if (currentMoveTry == 20)
                                    {
                                        AllowRunningPrevious();
                                    }
                                    else
                                    {
                                        SetDesc("IOException while moving new WA dir, retrying... (" + currentMoveTry + ")");
                                    }
                                }
                                Thread.Sleep(200);
                            }

                            //cleanup
                            var downloadedZip = new FileInfo(downloadPath);
                            if (downloadedZip.Exists)
                            {
                                try
                                {
                                    downloadedZip.Delete();
                                }
                                catch (Exception ex2)
                                {
                                    SetDesc("failed to clean the partially downloaded file: " + downloadPath +
                                            "\r\n" + ex2.Message, true);
                                }
                            }
                            try
                            {
                                tempExtractDir.Delete(true);
                            }
                            catch (Exception ex3)
                            {
                                SetDesc("failed to clean temp dir: " + tempExtractDir.FullName +
                                        "\r\n" + ex3.Message, true);
                                LogException(ex3);
                            }

                            //clean old versions
                            foreach (var dir in existingWaDirs)
                            {
                                try
                                {
                                    dir.Delete(true);
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogDiag("error while deleting old WA dir", this, ex);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SetFailure("Extracting failed", "Error while trying to extract archive: " + ex.Message +
                                "\r\n\r\nTry to run launcher again " +
                                (previousVersionAvailable ? " or start previous version" : ""), previousVersionAvailable);
                            failed = true;
                            LogException(ex);
                        }
                    }
                }

                if (!failed) RunLatestVersion();
            }
            catch (AggregateException agExc)
            {
                string error = "Unexpected errors while running launcher: " + string.Join(",", agExc.InnerExceptions.Select(x => x.Message));
                SetFailure("Launching failed", error);
                LogException(agExc);
            }
            catch (Exception exception)
            {
                SetFailure("Launching failed", "Unexpected error while running launcher: " + exception.Message);
                LogException(exception);
            }
        }

        void RunLatestVersion()
        {
            SetStatus("Starting Wurm Assistant");
            var workdir = GeneralHelper.PathCombineWithCodeBasePath(null);
            var dirInfo = new DirectoryInfo(workdir);
            var dirs = dirInfo.GetDirectories();
            Version localLatestVersion = null;
            DirectoryInfo runDir = null;
            foreach (var dir in dirs)
            {
                Match match = Regex.Match(dir.Name, @"WurmAssistant_(\d+)_(\d+)_(\d+)_(\d+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var thisVersion = new Version(
                        int.Parse(match.Groups[1].Value),
                        int.Parse(match.Groups[2].Value),
                        int.Parse(match.Groups[3].Value),
                        int.Parse(match.Groups[4].Value));
                    if (localLatestVersion == null || thisVersion > localLatestVersion)
                    {
                        localLatestVersion = thisVersion;
                        runDir = dir;
                    }
                }
            }

            if (runDir != null)
            {
                try
                {
                    Process.Start(Path.Combine(runDir.FullName, "WurmAssistant2.exe"));
                    this.BeginInvoke((Action)Close);
                }
                catch (Exception ex)
                {
                    SetFailure("Starting Wurm Assistant failed", "Error while trying to start Wurm Assistant: " + ex.Message);
                    LogException(ex);
                }
            }
            else
            {
                SetFailure("Starting Wurm Assistant failed", "Found no Wurm Assistant to run");
            }
        }

        private void SetStatus(string statusMessage)
        {
            Logger.LogInfo(statusMessage);
            labelHeader.BeginInvoke((Action)(() => labelHeader.Text = statusMessage));
        }

        private void SetDesc(string descriptionMessage, bool append = false)
        {
            Logger.LogInfo(descriptionMessage);
            labelDescription.BeginInvoke((Action)(() =>
            {
                if (append) labelDescription.Text += "\r\n" + descriptionMessage;
                else labelDescription.Text = descriptionMessage;
            }));
        }

        private void SetProgressBar(long currentValue, long maxValue)
        {
            progressBar1.BeginInvoke((Action)(() =>
            {
                progressBar1.Maximum = (int)maxValue;
                progressBar1.Value = (int)currentValue;
                labelDescription.Text = string.Format("Downloaded {0:F1} out of {1:F1} MB",
                    currentValue * 0.00000095367431640625, maxValue * 0.00000095367431640625);
            }));
        }

        private void SetProgressBarToBlocks()
        {
            progressBar1.BeginInvoke((Action)(() =>
            {
                progressBar1.Style = ProgressBarStyle.Blocks;
            }));
        }

        private void SetFailure(string failMessage, string descMessage = null, bool allowPreviousVersion = false)
        {
            Logger.LogError(failMessage + ", " + descMessage);
            this.BeginInvoke((Action)(() =>
            {
                progressBar1.Style = ProgressBarStyle.Blocks;
                labelHeader.ForeColor = Color.Red;
                labelHeader.Text = failMessage;
                if (descMessage != null) labelDescription.Text = descMessage;
            }));
            AllowRunningPrevious(allowPreviousVersion);
        }

        private void LogException(AggregateException aggException)
        {
            foreach (var exception in aggException.InnerExceptions)
            {
                Logger.LogDiag("", this, exception);
            }
        }

        private void LogException(Exception exception)
        {
            Logger.LogDiag("", this, exception);
        }

        private void AllowRunningPrevious(bool allow = true)
        {
            this.BeginInvoke((Action)(() =>
            {
                button1.Visible = allow;
            }));
        }

        private TimeSpan GetTimeout(int retry)
        {
            switch (retry)
            {
                case 1:
                    return TimeSpan.FromSeconds(5);
                case 2:
                    return TimeSpan.FromSeconds(10);
                default:
                    return TimeSpan.FromSeconds(15);
            }
        }
    }
}
