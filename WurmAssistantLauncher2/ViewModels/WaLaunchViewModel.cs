using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Aldurcraft.Spellbook40.WPF.Toolkit.Commands;
using Aldurcraft.WurmAssistantLauncher2.Managers;
using Aldurcraft.WurmAssistantLauncher2.Models;

namespace Aldurcraft.WurmAssistantLauncher2.ViewModels
{
    class WaLaunchViewModel
    {
        private readonly WaLaunchManager manager;

        public ICommand LaunchCommand { get; set; }

        public WaLaunchViewModel(BuildType buildType)
        {
            if (buildType == BuildType.Stable)
            {
                manager = new StableWaLaunchManager();
            }
            else if (buildType == BuildType.Beta)
            {
                manager = new BetaWaLaunchManager();
            }
            else
            {
                throw new LauncherException("Unknown build type");
            }

            LaunchCommand = new DelegateCommand(() =>
            {
                try
                {
                    manager.Launch();
                }
                catch (Exception exception)
                {
                    ErrorManager.ShowWarning(exception.Message, exception);
                }
            });

            App.LauncherTaskbarIcon.ContextMenu.Items.Add(
                new MenuItem()
                {
                    Command = LaunchCommand,
                    Header = string.Format("Launch {0} Wurm Assistant", buildType)
                });
        }
    }
}
