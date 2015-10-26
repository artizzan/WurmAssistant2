using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;

using Aldurcraft.Utility.PopupNotify;
using Aldurcraft.WurmOnline.WurmLogsManager;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Aldurcraft.Utility;
using Aldurcraft.WurmOnline.WurmAssistant2.Engine;
using Aldurcraft.WurmOnline.WurmLogsManager.Searcher;
using Aldurcraft.WurmOnline.WurmState;

namespace Aldurcraft.WurmOnline.WurmAssistant2
{
    static class AssistantEngine
    {
        [DataContract]
        public class AssistantSettings
        {
            [DataMember]
            public bool WizardCompleted = false;
            [DataMember]
            public bool NotifyOnNewFeatures = false;
            [DataMember]
            public bool ChangelogOnEveryUpdate = false;
            [DataMember]
            public System.Drawing.Point WindowSize = new System.Drawing.Point();
            [DataMember]
            public System.Drawing.Point WindowLocation = new System.Drawing.Point();
            [DataMember]
            public List<string> ModulesInUse = null;
            [DataMember]
            public Version previousAssistantVersion = null;
            [DataMember]
            public bool MiminizeToTray = false;
            [DataMember]
            public bool StartMinimized = false;
            [DataMember]
            public bool BallonTooltipShown = false;
            [DataMember]
            public string LastNewsUrlShown = null;
            [DataMember]
            public bool AlwaysShowNotifyIcon = false;
            [DataMember]
            public bool PromptOnExit = false;
            [DataMember]
            public List<OtherTool> OtherTools = null;
            [DataMember]
            public bool HideBeerButton;
            [DataMember]
            public bool WebFeedDisabled;
            [DataMember]
            public bool SearcherDbWipeScheduled;
            [DataMember]
            public string WurmDirOverride = null;
            [DataMember]
            public bool AssistantFuturePollDisplayed = false;
            [DataMember]
            public bool Wa3PromoDisplayed { get; set; }

            //nonpersisted, update to have assistant open this link on next launch
            public const string CurrentNewsUrl = @"http://forum.wurmonline.com/index.php?/topic/68031-wurm-assistant-2x-bundle-of-useful-tools/page-48#entry944197";
        }

        const string THIS = "AssistantEngine";

        public static PersistentObject<AssistantSettings> Settings = new PersistentObject<AssistantSettings>(new AssistantSettings());
        public static string DataDir { get; private set; }

        public static bool BeerButtonHidden
        {
            get { return Settings.Value.HideBeerButton; }
            set
            {
                Settings.Value.HideBeerButton = value;
                Settings.DelayedSave();
                AssistantForm.ButtonBuyBeerHidden = value;
            }
        }

        public static bool WebFeedDisabled
        {
            get { return Settings.Value.WebFeedDisabled; }
            set
            {
                Settings.Value.WebFeedDisabled = value;
                Settings.DelayedSave();
                WurmState.WurmServer.WebFeedDisabled = value;
            }
        }

        public static WurmAssistant AssistantForm;

        #region ENGINE INITIALIZATION AND UPDATE

        //first part of init, close program if this fails
        //all requirements for starting assistant should be here
        public static bool Init1_Settings(WurmAssistant mainForm)
        {
            AssistantForm = mainForm;
            SetupDataDirectories();
            Logger.SetLogSaveDir(Path.Combine(DataDir, "Logs"));
            Logger.SetTBOutput(AssistantForm.GetTextBoxForLog());
            Logger.SetConsoleHandlingMode(Logger.ConsoleHandlingOption.SendConsoleToLoggerOutputINFO);

            Logger.LogDiag("-------------");
            Logger.LogDiag("-------------");
            Logger.LogDiag("STARTING WURM ASSISTANT " + Assembly.GetEntryAssembly().GetName().Version.ToString());

            Settings.FilePath = Path.Combine(DataDir, "AssistantSettings.xml");
            Logger.LogDiag("loading settings");
            bool settingsLoaded = Settings.Load();

            AssistantForm.ButtonBuyBeerHidden = BeerButtonHidden;

            if (!settingsLoaded || !Settings.Value.WizardCompleted)
            {
                Logger.LogDiag("launching configuration wizard");
                FormConfigWizard wizard = new FormConfigWizard();
                if (wizard.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Logger.LogDiag("applying wizard choices");
                    ApplyWizardResults(wizard.results);
                    Settings.Value.WizardCompleted = true;
                    Logger.LogDiag("saving updated settings");
                    Settings.Save();
                    return true;
                }
                else return false;
            }

            if (Settings.Value.WurmDirOverride != null && !WurmState.WurmClient.InitSuccessful)
            {
                WurmState.WurmClient.OverrideWurmDir(Settings.Value.WurmDirOverride);
            }

            return true;
        }

        public static void ApplyWizardResults(FormConfigWizard.Results results)
        {
            if (results.OnUpdateShowNewFeatures != null)
                Settings.Value.NotifyOnNewFeatures = results.OnUpdateShowNewFeatures.Value;
            else Settings.Value.NotifyOnNewFeatures = false;

            if (results.OnUpdateShowFullChangelog != null)
                Settings.Value.ChangelogOnEveryUpdate = results.OnUpdateShowFullChangelog.Value;
            else Settings.Value.ChangelogOnEveryUpdate = false;

            if (results.OverrideWurmDir != null)
            {
                Settings.Value.WurmDirOverride = results.OverrideWurmDir;
            }
        }

        static void SetupDataDirectories()
        {
            // Wurm Unlimited tweak:
            string localAssemblyDir = GeneralHelper.PathCombineWithCodeBasePath("UserData");
            DataDir = localAssemblyDir;
        }

        //second part of init for starting engine
        //this happens right before update loop is enabled
        public static void Init2_Engine()
        {
            try
            {
                Logger.LogInfo("initializing popups lib");
                Aldurcraft.Utility.PopupNotify.Popup.Initialize();

                Logger.LogInfo("initializing sound engine");
                Aldurcraft.Utility.SoundEngine.SoundBank.InitializeSoundBank(Path.Combine(DataDir, "SoundBank"));

                Logger.LogInfo("Initializing player server tracker");
                WurmState.PlayerServerTracker.Initialize(Path.Combine(DataDir, "WurmState"));

                Logger.LogInfo("initializing wurm state lib");
                WurmState.WurmServer.WebFeedDisabled = WebFeedDisabled;
                WurmState.WurmServer.InitializeWurmServer(Path.Combine(DataDir, "WurmState"));

                Logger.LogInfo("initializing wurm logs parser");
                WurmLogs.AssignSynchronizingObject(AssistantForm);

                Logger.LogInfo("starting wurm logs monitor");
                WurmLogs.Enable();

                Logger.LogInfo("initializing log searching api");
                bool wipeExistingDb = Settings.Value.SearcherDbWipeScheduled;
                if (wipeExistingDb)
                {
                    Logger.LogInfo("Clearing existing searcher DB is scheduled");
                    Settings.Value.SearcherDbWipeScheduled = false;
                    Settings.DelayedSave();
                }
                WurmLogSearcherAPI.Initialize(DataDir, wipeExistingDb);

                Logger.LogInfo("initializing modules");
                Modules.Init();
            }
            catch (Exception exception)
            {
                Logger.LogCritical("problem initializing Assistant engine", THIS, exception);
            }
        }

        internal static void AfterInit()
        {
            Logger.LogInfo("checking for new assistant version");
            Version assistantVersion = Assembly.GetEntryAssembly().GetName().Version;
            HandleVersionUpdates(assistantVersion);
            //if (!Settings.Value.Wa3PromoDisplayed)
            //{
            //    DisplayWa3Promo();
            //    Settings.Value.Wa3PromoDisplayed = true;
            //}
        }

        public static void DisplayWa3Promo()
        {
            try
            {
                var promoForm = new Wa3PromoForm
                {
                    StartPosition = FormStartPosition.CenterScreen
                };
                promoForm.Show();
                promoForm.BringToFront();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        //update loop def 100ms
        internal static void Update()
        {
            try
            {
                Modules.Update();
                Settings.Update();
            }
            catch (Exception _e)
            {

                Logger.LogError("problem on update loop", THIS, _e);
            }
        }

        internal static void AppClosing()
        {
            try
            {
                Logger.LogDiag("STOPPING WURM ASSISTANT");
                Settings.Save();
                Modules.StopAll();
            }
            catch (Exception _e)
            {
                Logger.LogError("appclosing problem", THIS, _e);
            }
        }

        #endregion

        #region COMPONENT MANAGERS

        public static class Modules
        {
            internal static void Init()
            {
                ModuleManager.Init();
            }

            internal static void Update()
            {
                ModuleManager.Update();
            }

            internal static void AddButton(AssistantModule module)
            {
                AssistantForm.AddModuleButton(module);
            }

            internal static void RemoveButton(Type type)
            {
                AssistantForm.RemoveModuleButton(type);
            }

            internal static void StopAll()
            {
                ModuleManager.StopAll();
            }

            internal static void ConfigureModules()
            {
                ModuleManager.ConfigureModules();
            }
        }

        public static class ErrorCounter
        {
            static int lastErrorCount = 0;

            public static string GetUpdate()
            {
                try
                {
                    if (Logger.ErrorCount > lastErrorCount)
                    {
                        string output = String.Format("Errors: {0}\r\nCritical: {1}",
                            Logger.ErrorCount, Logger.ErrorCount == 0 ? "none" : Logger.CriticalErrorCount.ToString());
                        lastErrorCount = Logger.ErrorCount;
                        return output;
                    }
                    else return null;
                }
                catch (Exception _e)
                {
                    Logger.LogError("could not update error counts", THIS, _e);
                    return null;
                }
            }
        }

        #endregion

        internal static void OpenLogDir()
        {
            try
            {
                Process.Start(Logger.LogSaveDir);
            }
            catch (Exception _e)
            {
                Logger.LogError("failed to open full log", THIS, _e);
            }
        }

        internal static void OpenForumThread()
        {
            try
            {
                Process.Start(@"http://forum.wurmonline.com/index.php?/topic/68031-windows-tool-wurm-assistant-wa2-alpha-released/");
            }
            catch (Exception _e)
            {
                Logger.LogError("failed to open forum thread", THIS, _e);
            }
        }

        internal static void HandleVersionUpdates(Version assistantVersion)
        {
            Version prevVer = Settings.Value.previousAssistantVersion;
            if (prevVer != null)
            {
                if (assistantVersion > prevVer)
                {
                    Logger.LogDiag("new assistant version detected");
                    Version prevVerWithoutRevision = new Version(prevVer.Major, prevVer.Minor, prevVer.Build, 0);
                    Version currentVerWithoutRevision = new Version(
                        assistantVersion.Major, assistantVersion.Minor, assistantVersion.Build, 0);
                    if (Settings.Value.ChangelogOnEveryUpdate && currentVerWithoutRevision > prevVerWithoutRevision)
                    {
                        Logger.LogDiag("showing changelog due to build version increase and user setting");
                        try
                        {
                            FormChangelog ui = new FormChangelog(
                                File.ReadAllText(GeneralHelper.PathCombineWithCodeBasePath("CHANGELOG.txt")));
                            ui.Show();
                            if (AssistantForm.WindowState != System.Windows.Forms.FormWindowState.Minimized)
                                ui.Location = FormHelper.GetCenteredChildPositionRelativeToParentWorkAreaBound(ui, AssistantForm);
                        }
                        catch (Exception _e)
                        {
                            Logger.LogError("problem opening changelog", THIS, _e);
                        }
                    }

                    //Version prevVerWithoutBuild = new Version(prevVer.Major, prevVer.Minor, 0, 0);
                    //Version currentVerWithoutBuild = new Version(
                    //    assistantVersion.Major, assistantVersion.Minor, 0, 0);
                    //if (Settings.Value.NotifyOnNewFeatures && currentVerWithoutBuild > prevVerWithoutBuild)
                    //{
                    //    Logger.LogDiag("showing new features prompt due to minor version increase and user setting");
                    //    if (System.Windows.Forms.MessageBox.Show(
                    //        "Wurm Assistant has been updated to new version: " + assistantVersion + "\r\n" +
                    //        "Would you like to open WA blog and check what's new?",
                    //        "Assistant Update",
                    //        System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                    //    {
                    //        try
                    //        {
                    //            Process.Start("http://blog.aldurcraft.com/WurmAssistant/");
                    //        }
                    //        catch (Exception _e)
                    //        {
                    //            Logger.LogError("Failed to open web link to WA blog", THIS, _e);
                    //        }
                    //    }
                    //}
                }
            }

            if (Settings.Value.NotifyOnNewFeatures && Settings.Value.LastNewsUrlShown != AssistantSettings.CurrentNewsUrl)
            {
                if (Settings.Value.LastNewsUrlShown == null)
                {
                    // on first install last news url will be null, no sense to show latest news at this point
                    Settings.Value.LastNewsUrlShown = AssistantSettings.CurrentNewsUrl;
                }
                else
                {
                    Logger.LogDiag("following to news post due user settings");

                    try
                    {
                        Process.Start(AssistantSettings.CurrentNewsUrl);
                    }
                    catch (Exception _e)
                    {
                        Logger.LogError("Failed to open web link to WA news", THIS, _e);
                    }
                    Settings.Value.LastNewsUrlShown = AssistantSettings.CurrentNewsUrl;
                }
            }

            if (!Settings.Value.AssistantFuturePollDisplayed && DateTime.Now < new DateTime(2015, 1, 31).AddDays(14))
            {
                Popup.Schedule(
                    "Survey",
                    "Take part in a survey about Wurm Assistant future, follow the link in main window.",
                    10000);
                Settings.Value.AssistantFuturePollDisplayed = true;
            }

            Settings.Value.previousAssistantVersion = assistantVersion;
            Settings.DelayedSave();
        }

        public static void ScheduleSearcherDbWipeOnNextRun()
        {
            Settings.Value.SearcherDbWipeScheduled = true;
            Settings.Save();
        }
    }
}
