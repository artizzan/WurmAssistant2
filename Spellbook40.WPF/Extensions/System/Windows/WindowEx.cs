using System.Windows;

namespace Aldurcraft.Spellbook40.WPF.Extensions.System.Windows
{
    public static class WindowEx
    {
        public static void ShowThisDarnWindowDammitWpfEdition(this Window window)
        {
            window.Show();
            if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal;
            window.BringIntoView();
            window.Activate();
        }
    }
}
