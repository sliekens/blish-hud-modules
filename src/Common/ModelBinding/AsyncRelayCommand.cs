using System.Windows.Input;

namespace SL.Common.ModelBinding;

public sealed class AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute()
    {
        return canExecute == null || canExecute();
    }

    public bool CanExecute(object parameter)
    {
        return CanExecute();
    }

    public void Execute()
    {
        execute();
    }

    public void Execute(object parameter)
    {
        Execute();
    }
}