using System.Windows.Input;

namespace SL.Common.ModelBinding;

public sealed class AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null) : ICommand
{
    public bool CanExecute(object parameter)
    {
        return canExecute == null || canExecute();
    }

    public void Execute(object parameter)
    {
        execute();
    }

    public event EventHandler? CanExecuteChanged;

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}