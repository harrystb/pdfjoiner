using System;
using System.Windows.Input;

namespace pdfjoiner
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _executeAction;
        public DelegateCommand(Action<object> executeAction)
        {
            _executeAction = executeAction;
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add
            {
                return;
            }

            remove
            {
                return;
            }
        }

        public void Execute(object parameter) => _executeAction(parameter);

        public bool CanExecute(object parameter) => true;
        

    }
}
