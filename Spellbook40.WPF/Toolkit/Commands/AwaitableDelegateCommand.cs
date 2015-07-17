using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Aldurcraft.Spellbook40.WPF.Toolkit.Commands
{
    public interface IAsyncCommand : IAsyncCommand<object>
    {
    }

    public interface IAsyncCommand<in T> : IRaiseCanExecuteChanged
    {
        Task ExecuteAsync(T obj);
        bool CanExecute(object obj);
        ICommand Command { get; }
    }
    public class AwaitableDelegateCommand : AwaitableDelegateCommand<object>, IAsyncCommand
    {
        public AwaitableDelegateCommand(Func<Task> executeMethod)
            : base(o => executeMethod())
        {
        }

        public AwaitableDelegateCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod)
            : base(o => executeMethod(), o => canExecuteMethod())
        {
        }
    }

    public class AwaitableDelegateCommand<T> : IAsyncCommand<T>, ICommand
    {
        private readonly Func<T, Task> executeMethod;
        private readonly DelegateCommand<T> underlyingCommand;
        private bool isExecuting;

        public AwaitableDelegateCommand(Func<T, Task> executeMethod)
            : this(executeMethod, _ => true)
        {
        }

        public AwaitableDelegateCommand(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod)
        {
            this.executeMethod = executeMethod;
            underlyingCommand = new DelegateCommand<T>(x => { }, canExecuteMethod);
        }

        public async Task ExecuteAsync(T obj)
        {
            try
            {
                isExecuting = true;
                RaiseCanExecuteChanged();
                await executeMethod(obj);
            }
            finally
            {
                isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public ICommand Command { get { return this; } }

        public bool CanExecute(object parameter)
        {
            return !isExecuting && underlyingCommand.CanExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { underlyingCommand.CanExecuteChanged += value; }
            remove { underlyingCommand.CanExecuteChanged -= value; }
        }

        public async void Execute(object parameter)
        {
            await ExecuteAsync((T)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            underlyingCommand.RaiseCanExecuteChanged();
        }
    }
}
