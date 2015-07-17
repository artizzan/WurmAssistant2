using System;
using System.Windows;

namespace Aldurcraft.WurmAssistantLauncher2.Managers
{
    public static class ErrorManager
    {
        public static void ShowError(string message, Exception exception = null, bool verbose = false)
        {
            if (exception != null && verbose)
            {
                message += "\r\nexception: " + exception.Message;
                message += "\r\ntrace:" + exception.StackTrace;
            }
            LogExc(exception);
            MessageBox.Show(
                message,
                "Error", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }

        public static void ShowWarning(string message, Exception exception = null)
        {
            LogExc(exception);
            MessageBox.Show(
                message, 
                "Warning", 
                MessageBoxButton.OK, 
                MessageBoxImage.Warning);
        }

        public static void ShowInfo(string message, Exception exception = null)
        {
            LogExc(exception);
            MessageBox.Show(message, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        static void LogExc(Exception exception)
        {
            if (exception != null)
            {
                App.Logger.LogError("Auto-logged error", "WurmAssistantLauncher", exception);
            }
        }
    }
}
