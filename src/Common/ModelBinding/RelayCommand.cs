using System.Windows.Input;

namespace SL.Common.ModelBinding;

public sealed class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute()
    {
        return canExecute == null || canExecute();
    }

    bool ICommand.CanExecute(object parameter)
    {
        return CanExecute();
    }

    public void Execute()
    {
        execute();
    }

    void ICommand.Execute(object parameter)
    {
        Execute();
    }
}