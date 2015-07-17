using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Aldurcraft.Spellbook40.Extensions.System.Reflection;
using Aldurcraft.WurmAssistantLauncher2.Managers;
using Aldurcraft.WurmAssistantLauncher2.Models;
using Aldurcraft.WurmAssistantLauncher2.ViewModels;
using Awesomium.Core;
using Aldurcraft.Spellbook40.WPF.Converters;

namespace Aldurcraft.WurmAssistantLauncher2.Views
{
    /// <summary>
    /// Interaction logic for WaVersionView.xaml
    /// </summary>
    public partial class WaVersionView : UserControl
    {
        //private DispatcherTimer timer;

        private WaUpdaterViewModel updateViewModel;
        private WaBackupsViewModel backupViewModel;
        private WaLaunchViewModel launchViewModel;
        public WaVersionView()
        {
            InitializeComponent();
            WebCtrl.Source = Path.Combine(AssemblyEx.CodeBaseLocalDirPath, "Views", "WaitPage.html").ToUri();
            //WebCtrl.Source = new Uri(AppContext.WebApiBasePath + "WurmAssistant/LauncherNewsStable");

            //timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            //timer.Tick += (s, args) =>
            //{
            //    WebCtrl.Source = new Uri(AppContext.WebApiBasePath + "WurmAssistant/LauncherNewsStable");
            //};

            //timer.Start();
        }

        private bool launcherSiteSet = false;

        //private BuildType buildType = BuildType.Unknown;
        public BuildType BuildType
        {
            set 
            {
                updateViewModel = new WaUpdaterViewModel(value);
                updateViewModel.WebCtrlSourceChanged += (sender, args) =>
                {
                    WebCtrl.Source = updateViewModel.WebCtrlSource;
                    if (!launcherSiteSet)
                    {
                        launcherSiteSet = true;
                        WebCtrl.Source = updateViewModel.WebCtrlSource;
                    }
                    else
                    {
                        WebCtrl.Source = updateViewModel.WebCtrlSource;
                        WebCtrl.Reload(true);
                    }
                };

                UpdatePanel.DataContext = updateViewModel;
                UpdaterSettingsPanel.DataContext = updateViewModel;
                ReinstallPanel.DataContext = updateViewModel;

                backupViewModel = new WaBackupsViewModel(value);
                BackupPanel.DataContext = backupViewModel; 

                launchViewModel = new WaLaunchViewModel(value);
                LaunchButton.DataContext = launchViewModel;

                updateViewModel.StartTimer();
                backupViewModel.StartTimer();
            }
        }

        private void WaVersionView_OnLoaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
