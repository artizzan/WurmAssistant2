using System;
using System.IO;
using System.Windows;
using Aldurcraft.Persistent40;
using Aldurcraft.Spellbook40.Extensions.System.IO;
using Aldurcraft.Spellbook40.SimpleLogger;
using Aldurcraft.Spellbook40.WCF.Pipes;
using Aldurcraft.Spellbook40.WizardTower;
using Aldurcraft.WurmAssistantLauncher2.Managers;
using Aldurcraft.WurmAssistantLauncher2.Models;
using Aldurcraft.WurmAssistantLauncher2.Utility;
using Aldurcraft.WurmAssistantMutexes;
using Hardcodet.Wpf.TaskbarNotification;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Aldurcraft.WurmAssistantLauncher2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static WurmAssistantLauncherGateway uniqueLaunchGateway;
        public static PipeCom LauncherPipeCom;

        public static SimpleLogger Logger;
        public static PersistentFactory PersistentFactory;
        public static TaskbarIcon LauncherTaskbarIcon { get; private set; }
        public static Persistent<LauncherSettings> Settings { get; private set; }
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                Current.Dispatcher.UnhandledException +=
                    (o, args) =>
                    {
                        ErrorManager.ShowError("Something unexpected happened in the launcher, please report this error.", args.Exception, true);
                        args.Handled = true;
                    };

                uniqueLaunchGateway = new WurmAssistantLauncherGateway(doNotAutoEnter: true);

                uniqueLaunchGateway.Enter(1000);

                LauncherPipeCom = new PipeCom(uniqueLaunchGateway.UniqueId, "Default");
                LauncherPipeCom.LoginAsEndpointAlpha();

                LauncherTaskbarIcon = (TaskbarIcon)FindResource("LauncherTaskbarIcon");

                AppContext.BuildContext();

                Logger = new SimpleLogger();
                Logger.SetLogSaveDir(PathEx.CombineWithCodeBase("LauncherLogs"));
                Logger.SetConsoleHandlingMode(SimpleLogger.ConsoleHandlingOption.SendConsoleToLoggerOutputDIAG);
                SpellbookLogger.Logged += (o, args) =>
                {
                    switch (args.Severity)
                    {
                        case LogSeverity.Debug:
                            Logger.LogDebug(args.Message, args.Source, args.Exception);
                            break;
                        case LogSeverity.Info:
                            Logger.LogInfo(args.Message, args.Source, args.Exception);
                            break;
                        case LogSeverity.Error:
                            Logger.LogError(args.Message, args.Source, args.Exception);
                            break;
                    }
                };
                var logger = new PersistentLogger();
                var storage = new PlainFilePersistentStorage(logger, AppContext.LauncherSettingsDir);
                var serializer = new JsonPersistentSerializer(logger);
                PersistentFactory = new PersistentFactory(storage, serializer, logger);

                var initialSetup = new InitialSetupManager();
                initialSetup.Execute();

                Settings = PersistentFactory.Create<LauncherSettings>("LauncherSettings");

                StartupUri = new Uri(@"Views\MainWindow.xaml", UriKind.Relative);
            }
            catch (GatewayClosedException)
            {
                // try activate existing instance of launcher
                using (var pipecom = new PipeCom(uniqueLaunchGateway.UniqueId, "Default"))
                {
                    pipecom.LoginAsAlphaClient();
                    pipecom.TrySend("ShowWindow", null);
                }
                Shutdown();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Unexpected error while starting the Launcher, please report this bug! Error: " +
                                exception.Message);
                Logger.LogError("error on app init", this, exception);
                Shutdown();
            }
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            if (LauncherPipeCom != null)
            {
                LauncherPipeCom.Dispose();
            }
            uniqueLaunchGateway.Dispose();
        }
    }
}
