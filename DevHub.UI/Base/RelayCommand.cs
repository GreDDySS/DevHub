using System.Windows.Input;

namespace DevHub.UI.Base
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?>? _executeWithParam;
        private readonly Action? _executeWithoutParam;
        private readonly Func<object?, bool>? _canExecute;
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _executeWithParam = execute;
            _canExecute = canExecute;
        }

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _executeWithoutParam = execute;
            if (canExecute != null)
            {
                _canExecute = _ => canExecute();
            }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter)
        {
            if (_executeWithParam != null)
                _executeWithParam(parameter);
            else
                _executeWithoutParam?.Invoke();
        }

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
