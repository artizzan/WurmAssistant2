using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Aldurcraft.Spellbook40.Extensions.System;
using Aldurcraft.Spellbook40.WPF.Toolkit.Commands;
using Aldurcraft.WurmAssistantLauncher2.Managers;
using Aldurcraft.WurmAssistantLauncher2.Models;
using Aldurcraft.WurmAssistantLauncher2.Views;
using WurmAssistantLauncher2.Annotations;

namespace Aldurcraft.WurmAssistantLauncher2.ViewModels
{
    public class WaBackupsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<WaBackup> WaBackups { get; set; }

        private DispatcherTimer timer;

        private DateTime lastBackupDate;

        public string LastBackup
        {
            get { return lastBackup; }
            set
            {
                if (value == lastBackup) return;
                lastBackup = value;
                OnPropertyChanged();
            }
        }
        public IAsyncCommand CreateBackupCommand { get; private set; }
        public ICommand ManageBackupsCommand { get; private set; }
        public ICommand ImportSettingsCommand { get; private set; }

        private readonly BackupsManager manager;
        private string lastBackup;

        private bool makingBackup;
        private string createBackupStatus;

        public string Title { get; private set; }

        public string CreateBackupStatus
        {
            get { return createBackupStatus; }
            private set
            {
                if (value == createBackupStatus) return;
                createBackupStatus = value;
                OnPropertyChanged();
            }
        }

        public WaBackupsViewModel(BuildType buildType)
        {
            if (buildType == BuildType.Stable)
            {
                manager = new StableBackupsManager(this);
            }
            else if (buildType == BuildType.Beta)
            {
                manager = new BetaBackupsManager(this);
            }
            else
            {
                throw new LauncherException("Unknown build type");
            }

            CreateBackupStatus = "Create backup";

            CreateBackupCommand = new AwaitableDelegateCommand(async () =>
            {
                try
                {
                    CreateBackupStatus = "Creating...";
                    makingBackup = true;
                    var t = new Task<WaBackup>(() => manager.CreateBackup());
                    t.Start();
                    var backup = await t;
                    WaBackups.Add(backup);
                }
                catch (Exception exception)
                {
                    App.Logger.LogError("", this, exception);
                    ErrorManager.ShowWarning(exception.Message, exception);
                }
                finally
                {
                    makingBackup = false;
                    CreateBackupStatus = "Create backup";
                    UpdateLastBackupTime();
                }
            }, () => !makingBackup);

            ManageBackupsCommand = new DelegateCommand(() =>
            {
                var ui = new ManageWaBackups(this);
                ui.ShowDialog();
            });

            ImportSettingsCommand = new DelegateCommand(() =>
            {
                // nothing yet
            });

            Title = string.Format("Backups manager ({0})", buildType.ToString());
            RefreshBackups();

            UpdateLastBackupTime();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(30);
            timer.Tick += (sender, args) => UpdateLastBackupForDisplay();

            WaBackups.CollectionChanged += (sender, args) => UpdateLastBackupTime();
        }

        public void StartTimer()
        {
            timer.Start();
        }

        void UpdateLastBackupTime()
        {
            if (WaBackups.Any())
            {
                lastBackupDate = WaBackups.Max(x => x.TimeStamp);
            }
            else
            {
                lastBackupDate = DateTime.MinValue;
            }
            UpdateLastBackupForDisplay();
        }

        void UpdateLastBackupForDisplay()
        {
            LastBackup = lastBackupDate == DateTime.MinValue
                ? "Last backup: never" 
                : string.Format("Last backup: {0} ago", (DateTime.Now - lastBackupDate).FormatConciseToMinutesEx());
        }

        private void RefreshBackups()
        {
            WaBackups = new ObservableCollection<WaBackup>(manager.GetBackups());
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
