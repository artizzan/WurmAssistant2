using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Aldurcraft.Spellbook40.Transient;
using Aldurcraft.Spellbook40.WebApi;
using Aldurcraft.Spellbook40.WPF.Toolkit.Commands;
using Aldurcraft.WurmAssistantLauncher2.Managers;
using Aldurcraft.WurmAssistantLauncher2.Models;
using Aldurcraft.WurmAssistantLauncher2.Utility;
using WurmAssistantLauncher2.Annotations;

namespace Aldurcraft.WurmAssistantLauncher2.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private DispatcherTimer timer;

        public MainWindowViewModel()
        {
            DownloadNewLauncherCommand = new AwaitableDelegateCommand(async () =>
            {
                try
                {
                    var requestUrl = string.Format("ProjectApi/LatestLauncherDownloadUrl/{0}", AppContext.ProjectName);
                    var newVersionUri = await TransientHelper.CompensateAsync(
                        () => WebApiEx.GetObjectFromWebApiAsync<string>(
                            AppContext.WebApiBasePath,
                            requestUrl,
                            TimeSpan.FromSeconds(15)),
                        "error while contacting web service",
                        5);
                    if (!string.IsNullOrWhiteSpace(newVersionUri))
                    {
                        Process.Start(newVersionUri);
                    }
                    else
                    {
                        throw new LauncherException("returned string was empty, requested at: " + requestUrl);
                    }
                }
                catch (Exception exception)
                {
                    App.Logger.LogError("problem: ", this, exception);
                    ErrorManager.ShowWarning("Failed to get download link for latest version", exception);
                }
            });

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);

            timer.Tick += async (sender, args) =>
            {
                timer.Interval = TimeSpan.FromHours(2);
                try
                {
                    NewVersionAvailable = await IsLauncherUpdateAvailableAsync();
                }
                catch (Exception exception)
                {
                    App.Logger.LogError("Failed to obtain latest launcher version", this, exception);
                }
            };
            timer.Start();
        }

        private bool newVersionAvailable;
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
        public async Task<bool> IsLauncherUpdateAvailableAsync()
        {
            // ask web service if available
            var resultString = await TransientHelper.CompensateAsync(
                () => WebApiEx.GetObjectFromWebApiAsync<string>(
                    AppContext.WebApiBasePath,
                    string.Format("ProjectApi/LatestLauncherVersion/{0}", AppContext.ProjectName),
                    TimeSpan.FromSeconds(15)),
                "Failed to check latest WA launcher version",
                5);
            var latestRemote = new Version(resultString);
            var latestLocal = Assembly.GetExecutingAssembly().GetName().Version;

            return latestLocal < latestRemote;
        }

        public ICommand DownloadNewLauncherCommand { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
