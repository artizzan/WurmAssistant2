using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Aldurcraft.Spellbook40.WPF.Toolkit.Commands
{
    public static class CommandExtensions
    {
        public static void RaiseCanExecuteChanged(this ICommand command)
        {
            var canExecuteChanged = command as IRaiseCanExecuteChanged;

            if (canExecuteChanged != null)
                canExecuteChanged.RaiseCanExecuteChanged();
        }
    }
}
