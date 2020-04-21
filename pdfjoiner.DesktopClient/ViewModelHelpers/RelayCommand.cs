using System;
using System.Windows.Input;

namespace pdfjoiner.DesktopClient
{
    public class RelayCommand : ICommand
    {
        private readonly Action _Action;

        /// <summary>
        /// Event that is fired when the <see cref="CanExecute(object)"/> value has changed.
        /// </summary>
        public event EventHandler CanExecuteChanged = (sender, e) => { };

        public RelayCommand(Action action)
        {
            _Action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _Action();
        }
    }
}
