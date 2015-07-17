using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using Aldurcraft.WurmAssistantLauncher2.Managers;
using Aldurcraft.WurmAssistantLauncher2.Models;
using Aldurcraft.WurmAssistantMutexes;
using Hardcodet.Wpf.TaskbarNotification;
using WurmAssistantLauncher2.Annotations;
using Aldurcraft.Spellbook40.WPF.Toolkit.Commands;

namespace Aldurcraft.WurmAssistantLauncher2.ViewModels
{
    public class WaUpdaterViewModel : INotifyPropertyChanged
    {
        public WaUpdaterViewModel(BuildType buildType)
        {
            if (buildType == BuildType.Stable)
            {
                manager = new StableWaUpdateManager();
            }
            else if (buildType == BuildType.Beta)
            {
                manager = new BetaWaUpdateManager();
            }
            else
            {
                throw new LauncherException("Unknown build type");
            }

            Settings = App.PersistentFactory.Create<UpdaterSettings>("UpdaterSettings" + buildType);
            Settings.Data.PropertyChanged += (sender, args) =>
                Settings.RequireSave();
            Application.Current.Exit += (sender, args) =>
                Settings.Save();

            UpdateCommand = new AwaitableDelegateCommand(async () =>
            {
                try
                {
                    await manager.UpdateAsync();
                    NewVersionAvailable = false;
                    nextUpdateCheck = DateTime.Now;
                }
                catch (Exception exception)
                {
                    App.Logger.LogError("update error", this, exception);
                    ErrorManager.ShowWarning("update error: " + exception.Message, exception);
                }
                manager.StatusReset();
            }, () => !UpdateInProgress);

            ReinstallCommand = new AwaitableDelegateCommand(async () =>
            {
                try
                {
                    manager.StatusReset();
                    AvailabilityStatus = string.Empty;
                    await manager.UninstallAsync();
                    await UpdateCommand.ExecuteAsync(null);
                }
                catch (Exception exception)
                {
                    App.Logger.LogError("reinstall error", this, exception);
                    ErrorManager.ShowWarning("reinstall error: " + exception.Message, exception);
                }
            });

            manager.UpdateStatusChanged += (sender, args) =>
            {
                UpdateStatus = args.Status;
                UpdateInProgress = args.IsRunning;
                UpdateProgressIndeterminate = args.ProgressIndeterminate;
                UpdateProgress = args.Progress;
            };

            AvailabilityStatus = "Checking for updates...";

            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };

            timer.Tick += async (s, a) =>
            {
                timer.Interval = TimeSpan.FromSeconds(10);

                if (!initialNewsLoad)
                {
                    initialNewsLoad = true;
                    RefreshWaNews();
                }

                Settings.Update();
                if (!UpdateInProgress && DateTime.Now > nextUpdateCheck)
                {
                    nextUpdateCheck = DateTime.Now + TimeSpan.FromHours(6);

                    try
                    {
                        CheckingVersion = true;
                        AvailabilityStatus = "Checking for updates...";
                        await CheckForNewVersion();
                        CheckingVersion = false;

                        if (newVersionAvailable)
                        {
                            RefreshWaNews();
                            manager.StatusReset();
                            AvailabilityStatus = string.Empty;
                            // run auto update if applicable
                            if (Settings.Data.NotifyOnNewVersion)
                            {
                                // todo show baloon, max once each 16 hours
                                if (DateTime.Now > nextUpdateReminder)
                                {
                                    var message = string.Format("New {0} Wurm Assistant version is available!", buildType);
                                    App.LauncherTaskbarIcon.ShowBalloonTip("Rejoice!", message, BalloonIcon.Info);
                                    nextUpdateReminder = DateTime.Now + TimeSpan.FromHours(16);
                                }
                            }
                            if (Settings.Data.AutoUpdate)
                            {
                                try
                                {
                                    await manager.UpdateAsync();
                                    NewVersionAvailable = false;
                                    nextUpdateCheck = DateTime.Now;
                                    var message =
                                        string.Format("{0} Wurm Assistant has just been updated to new version!",
                                            buildType);
                                    // show tray pop
                                    App.LauncherTaskbarIcon.ShowBalloonTip("Rejoice!", message, BalloonIcon.Info);
                                }
                                catch (Exception exception)
                                {
                                    // handle exceptions silently
                                    App.Logger.LogError("auto update failed, " + buildType, this, exception);
                                }
                                manager.StatusReset();
                            }
                        }
                        else
                        {
                            manager.StatusReset();
                            AvailabilityStatus = "WA-" + buildType + " is up to date";
                        }
                    }
                    catch (Exception exception)
                    {
                        App.Logger.LogError("Update error", this, exception);
                        AvailabilityStatus = "Error: " + exception.Message +
                            ". Try checking internet connection, firewall settings or restarting the launcher."
                            + " Version check will retry in 5 minutes.";
                        nextUpdateCheck = DateTime.Now + TimeSpan.FromMinutes(5);
                    }
                }
            };
        }

        #region Commands

        public IAsyncCommand UpdateCommand { get; private set; }
        public IAsyncCommand ReinstallCommand { get; private set; }

        #endregion

        #region Properties

        public Uri WebCtrlSource
        {
            get { return webCtrlSource; }
            set
            {
                if (Equals(value, webCtrlSource)) return;
                webCtrlSource = value;
                var eh = WebCtrlSourceChanged;
                if (eh != null)
                {
                    WebCtrlSourceChanged(this, EventArgs.Empty);
                }
                OnPropertyChanged("WebCtrlSource");
            }
        }

        public bool CheckingVersion
        {
            get { return checkingVersion; }
            set
            {
                if (value.Equals(checkingVersion)) return;
                checkingVersion = value;
                OnPropertyChanged("CheckingVersion");
            }
        }

        public string AvailabilityStatus
        {
            get { return availabilityStatus; }
            set
            {
                if (value == availabilityStatus) return;
                availabilityStatus = value;
                OnPropertyChanged("AvailabilityStatus");
            }
        }

        /// <summary>
        /// New WA version is available for update
        /// </summary>
        public bool NewVersionAvailable
        {
            get
            {
                return newVersionAvailable;
            }
            set
            {
                if (value.Equals(newVersionAvailable)) return;
                newVersionAvailable = value;
                OnPropertyChanged("NewVersionAvailable");
            }
        }

        /// <summary>
        /// WA is being updated at the moment
        /// </summary>
        public bool UpdateInProgress
        {
            get { return updateInProgress; }
            set
            {
                if (value.Equals(updateInProgress)) return;
                updateInProgress = value;
                OnPropertyChanged("UpdateInProgress");
            }
        }

        /// <summary>
        /// Status of the update
        /// </summary>
        public string UpdateStatus
        {
            get { return updateStatus; }
            set
            {
                if (value == updateStatus) return;
                updateStatus = value;
                OnPropertyChanged("UpdateStatus");
            }
        }

        /// <summary>
        /// Progress indicator for measurable update operation, between 0 and 1000
        /// </summary>
        public int UpdateProgress
        {
            get { return updateProgress; }
            set
            {
                if (value == updateProgress) return;
                if (value < 0 || value > 1000) throw new LauncherException("value out of range: UpdateProgress, " + value);
                updateProgress = value;
                OnPropertyChanged("UpdateProgress");
            }
        }

        /// <summary>
        /// Current update operation has no measurable progress
        /// </summary>
        public bool UpdateProgressIndeterminate
        {
            get { return updateProgressIndeterminate; }
            set
            {
                if (value.Equals(updateProgressIndeterminate)) return;
                updateProgressIndeterminate = value;
                OnPropertyChanged("UpdateProgressIndeterminate");
            }
        }

        #endregion

        #region Public

        public event EventHandler WebCtrlSourceChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public Persistent40.Persistent<UpdaterSettings> Settings { get; private set; }

        public void StartTimer()
        {
            timer.Start();
        }

        #endregion

        #region internals

        private DispatcherTimer timer;
        private WaUpdateManager manager;

        private DateTime nextUpdateCheck;
        private DateTime nextUpdateReminder;

        private bool initialNewsLoad = false;

        private bool newVersionAvailable;
        private bool updateInProgress;
        private string updateStatus;
        private int updateProgress;
        private bool updateProgressIndeterminate;
        private string availabilityStatus;
        private bool checkingVersion;
        private Uri webCtrlSource;

        void RefreshWaNews()
        {
            WebCtrlSource = new Uri(AppContext.WebApiBasePath + "WurmAssistant/LauncherNews" + manager.BuildType);
        }

        private async Task CheckForNewVersion()
        {
            NewVersionAvailable = await manager.IsUpdateAvailableAsync();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public class UpdaterSettings : INotifyPropertyChanged
        {
            private bool autoUpdate;
            private bool notifyOnNewVersion;

            public bool AutoUpdate
            {
                get { return autoUpdate; }
                set
                {
                    if (value.Equals(autoUpdate)) return;
                    autoUpdate = value;
                    OnPropertyChanged();
                }
            }

            public bool NotifyOnNewVersion
            {
                get { return notifyOnNewVersion; }
                set
                {
                    if (value.Equals(notifyOnNewVersion)) return;
                    notifyOnNewVersion = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
