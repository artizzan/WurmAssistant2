using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aldurcraft.DevTools
{
    abstract class PublishBaseOperation : Operation
    {
        private readonly ArgsManager argsManager;
        private readonly string outputDir;

        private Robot robot;
        private readonly string projDirRoot;

        protected string TargetPublishDir;

        const int MaxWebPublishTries = 5;
        const int WebPublishRetrySleepMillis = 10000;
        readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(600);

        const string ProjectName = "WurmAssistant";

        protected PublishBaseOperation(Robot robot, ArgsManager argsManager)
        {
            this.robot = robot;
            this.argsManager = argsManager;
            BuildConfig = argsManager.GetArg(ArgType.BuildConfig);
            outputDir = argsManager.GetArg(ArgType.OutputDir);
            ValidateDirExists(outputDir);
            projDirRoot = argsManager.GetArg(ArgType.ProjDirRoot);
            ValidateDirExists(projDirRoot);
        }

        private string WebPublishBasePath
        {
            get
            {
                //return "http://localhost:19296/";
                //return "http://test.aldurcraft.com/";
                //return "http://aldurcraft.com/";
                return "http://old.aldurcraft.com/";
            }
        }

        // get version
        protected Version GetVersion()
        {
            return Helper.GetVersionFromAssemblyInfo(projDirRoot);
        }
        //create temp program dir
        protected DirectoryInfo CreateTempProgramDir(Version version)
        {
            string pathFrom = outputDir;
            string pathTo = TargetPublishDir;

            //publish operation

            string newFileDir = string.Format("WurmAssistant_{0}_{1}", BuildConfig, version.ToString().Replace(".", "_"));
            string releaseVersionedDir = Path.Combine(pathTo, newFileDir);
            WriteOut("creating new publish directory: " + releaseVersionedDir);
            if (Directory.Exists(releaseVersionedDir)) throw new Exception("directory already exists");
            Directory.CreateDirectory(releaseVersionedDir);
            WriteOut("copying contents, skipping app.publish dir if exists");
            Helper.DirectoryCopy(pathFrom, releaseVersionedDir, true, new[] { "app.publish" }, new[] { ".pdb" });

            return new DirectoryInfo(releaseVersionedDir);
        }

        protected DirectoryInfo CreateTempProgramDirForOldApi(Version version)
        {
            string pathFrom = outputDir;
            string pathTo = TargetPublishDir;

            //publish operation

            string newFileDir = string.Format("WurmAssistant_{0}", version.ToString().Replace(".", "_"));
            string releaseVersionedDir = Path.Combine(pathTo, newFileDir);
            WriteOut("creating new publish directory for old api: " + releaseVersionedDir);
            if (Directory.Exists(releaseVersionedDir)) throw new Exception("directory already exists");
            Directory.CreateDirectory(releaseVersionedDir);
            WriteOut("copying contents, skipping app.publish dir if exists");
            Helper.DirectoryCopy(pathFrom, releaseVersionedDir, true, new[] { "app.publish" }, new[] { ".pdb" });

            return new DirectoryInfo(releaseVersionedDir);
        }
        // create zipped program file
        protected FileInfo CreateZippedProgramFile(DirectoryInfo tempProgramDir)
        {
            string appPath = Path.GetDirectoryName((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath);
            WriteOut("beginning zipping");
            string newFileFullPath = tempProgramDir + ".zip";
            Process process = Process.Start(Path.Combine(appPath, "7zip", "7za.exe"),
                String.Format("a -tzip {0} {1}", newFileFullPath, tempProgramDir));

            if (!process.WaitForExit(30000))
            {
                WriteOut("zip timed out (30000)");
                throw new RobotException("Zipping timed out");
            }

            WriteOut("zip complete");

            var fileinfo = new FileInfo(newFileFullPath);
            if (fileinfo.Exists == false)
            {
                WriteOut("zipped file appears to be missing: " + newFileFullPath);
                throw new RobotException("Zipped file appears to be missing");
            }

            return fileinfo;
        }
        
        protected void PublishToOldWebApi(FileInfo zippedFile)
        {
            int currentTry = 1;
            while (true)
            {
                try
                {
                    WriteOut("attempting to web-publish (try " + currentTry + ")");
                    using (var client = HttpClientFactory.Create())
                    {
                        client.BaseAddress = new Uri(WebPublishBasePath);
                        client.DefaultRequestHeaders.Add("AccessControl", Helper.AuthToken);
                        client.Timeout = defaultTimeout;
                        using (var content = new MultipartFormDataContent())
                        {
                            var fileContent = new StreamContent(File.OpenRead(zippedFile.FullName));
                            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                            {
                                FileName = zippedFile.Name
                            };
                            content.Add(fileContent);

                            WriteOut("sending file");
                            var result = client.PostAsync("api/WurmAssistantApi", content).Result;
                            WriteOut(result.ReasonPhrase);
                        }
                        break;
                    }
                }
                catch (Exception ex)
                {
                    string mess;
                    var exception = ex as AggregateException;
                    if (exception != null)
                    {
                        mess = string.Join(", ",
                            exception.InnerExceptions.Select(
                                x => x.Message + " (" + x.StackTrace + ")"));
                    }
                    else
                    {
                        mess = ex.Message + " (" + ex.StackTrace + ")";
                    }
                    WriteOut("Error occurred: " + mess);
                    currentTry++;
                    if (currentTry > MaxWebPublishTries)
                    {
                        WriteOut("Could not perform old web publish");
                        throw new RobotException("could not perform old web publish");
                    }
                    Thread.Sleep(WebPublishRetrySleepMillis);
                }
            }
            WriteOut("sending finished");
        }
        
        protected void PublishToNewWebApi(FileInfo zippedFile, Version version)
        {
            WriteOut("attempting new web publish");

            int currentTry = 1;
            while (true)
            {
                try
                {
                    using (var client = HttpClientFactory.Create())
                    {
                        client.BaseAddress = new Uri(WebPublishBasePath);
                        client.DefaultRequestHeaders.Add("AccessControl", Helper.AuthToken);
                        client.Timeout = defaultTimeout;
                        using (var content = new MultipartFormDataContent())
                        {
                            var fileContent = new StreamContent(zippedFile.OpenRead());
                            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                            {
                                FileName = zippedFile.Name
                            };
                            content.Add(fileContent);
                            var publishUrl = string.Format(@"{0}/{1}/{2}?version={3}",
                                        "ProjectApi", ProjectName, BuildConfig, version);
                            WriteOut("Publishing to: " + WebPublishBasePath + "/" + publishUrl);
                            var result =
                                client.PostAsync(publishUrl, content)
                                    .Result;
                            WriteOut("New web api publish result: " + result.ReasonPhrase);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    string mess;
                    var exception = ex as AggregateException;
                    if (exception != null)
                    {
                        mess = string.Join(", ",
                            exception.InnerExceptions.Select(
                                x => x.Message + " (" + x.StackTrace + ")"));
                    }
                    else
                    {
                        mess = ex.Message + " (" + ex.StackTrace + ")";
                    }
                    WriteOut("Error occurred: " + mess);
                    currentTry++;
                    if (currentTry > MaxWebPublishTries)
                    {
                        WriteOut("Could not perform new web publish");
                        throw new RobotException("could not perform new web publish");
                    }
                    Thread.Sleep(WebPublishRetrySleepMillis);
                }
            }
        }

        [Obsolete("Feature replaced with manually written features")]
        protected void UpdateChangelogInNewApi(DirectoryInfo tempProgramDir)
        {
            string contents = File.ReadAllText(Path.Combine(tempProgramDir.FullName, "CHANGELOG.txt"));

            int currentTry = 1;
            while (true)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(WebPublishBasePath);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Add("AccessControl", Helper.AuthToken);

                        var response = client.PostAsJsonAsync(string.Format(@"{0}?projectName={1}&buildType={2}",
                            "api/ProjectChangelogApi", ProjectName, BuildConfig), contents)
                            .Result; //this is intended
                        WriteOut("changelog update result: " + response.ReasonPhrase);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    string mess;
                    var exception = ex as AggregateException;
                    if (exception != null)
                    {
                        mess = string.Join(", ",
                            exception.InnerExceptions.Select(
                                x => x.Message + " (" + x.StackTrace + ")"));
                    }
                    else
                    {
                        mess = ex.Message + " (" + ex.StackTrace + ")";
                    }
                    WriteOut("Error occurred: " + mess);
                    currentTry++;
                    if (currentTry > MaxWebPublishTries)
                    {
                        WriteOut("Could not perform changelog update");
                        throw new RobotException("could not perform changelog update");
                    }
                    Thread.Sleep(WebPublishRetrySleepMillis);
                }
            }
        }
    }
}
