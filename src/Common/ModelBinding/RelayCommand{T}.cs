using System.Windows.Input;

namespace SL.Common.ModelBinding;

public sealed class RelayCommand<T>(Action<T> execute, Func<T, bool>? canExecute = null) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(T parameter)
    {
        return canExecute == null || canExecute(parameter);
    }

    bool ICommand.CanExecute(object parameter)
    {
        return CanExecute((T)parameter);
    }

    public void Execute(T parameter)
    {
        execute(parameter);
    }

    void ICommand.Execute(object parameter)
    {
        Execute((T)parameter);
    }

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}