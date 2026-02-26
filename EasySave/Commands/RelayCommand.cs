using System;
using System.Windows.Input;

namespace EasySave.Commands
{
     
    /// A command whose sole purpose is to relay its functionality to other
    /// objects by invoking delegates.
     
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

         
        /// Creates a new command that can always execute.
         
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

         
        /// Creates a new command.
        
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

         
        /// Determines whether this command can execute in its current state.
         
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

         
        /// Occurs when changes occur that affect whether or not the command should execute.
         
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

         
        /// Executes the command.
         
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}