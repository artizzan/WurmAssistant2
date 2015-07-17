using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using Aldurcraft.Spellbook40.Extensions.System.IO;
using Aldurcraft.Spellbook40.WPF.Extensions.System.Windows;
using Aldurcraft.Spellbook40.WPF.Toolkit.Commands;
using Aldurcraft.WurmAssistantLauncher2.Managers;
using Aldurcraft.WurmAssistantLauncher2.Models;
using Aldurcraft.WurmAssistantLauncher2.ViewModels;
using Awesomium.Core;

namespace Aldurcraft.WurmAssistantLauncher2.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();

            this.Title = string.Format("Wurm Assistant Launcher ({0})", AppContext.ProgramVersion);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var tv = new TestView();
            tv.Show();
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private bool launcherSiteSet = false;

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Banner.Source = PathEx.CombineWithCodeBase("Views", "WaitPage.html").ToUri();
            //StableVersionView.DataContext = new WaUpdaterViewModel(BuildType.Stable);
            //BetaVersionView.DataContext = new WaUpdaterViewModel(BuildType.Beta);
            //Banner.Source = PathEx.CombineWithCodeBase("Views", "WaitPage.html").ToUri();
            //Banner.Source= new Uri("http://localhost:19296/WurmAssistant/LauncherBanner");

            DataContext = new MainWindowViewModel();

            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, args) =>
            {
                timer.Interval = TimeSpan.FromHours(6);
                if (!launcherSiteSet)
                {
                    launcherSiteSet = true;
                    Banner.Source = new Uri(AppContext.WebApiBasePath + "WurmAssistant/LauncherBanner");
                }
                else
                {
                    Banner.Source = new Uri(AppContext.WebApiBasePath + "WurmAssistant/LauncherBanner");
                    Banner.Reload(true);
                }
            };

            timer.Start();

            App.LauncherTaskbarIcon.TrayLeftMouseDown += (o, args) => this.ShowThisDarnWindowDammitWpfEdition();
            App.LauncherPipeCom.MessageReceived += (o, args) =>
            {
                if (args.MessageId == "ShowWindow")
                {
                    this.ShowThisDarnWindowDammitWpfEdition();
                }
                else App.Logger.LogInfo("Unhandled pipe message: " + args.MessageId);
            };

            //App.LauncherTaskbarIcon.LeftClickCommand = new DelegateCommand(() =>
            //{
            //    Show();
            //    if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
            //});
            //App.LauncherTaskbarIcon.DoubleClickCommand = App.LauncherTaskbarIcon.LeftClickCommand;
        }
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(e.Uri.OriginalString);
            }
            catch (Exception exception)
            {
                ErrorManager.ShowWarning("problem handling the operation: " + exception.Message, exception);
            }
        }
    }
}
