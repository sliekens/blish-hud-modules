using System.Windows.Input;

namespace SL.Common.ModelBinding;

public sealed class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
    public bool CanExecute()
    {
        return CanExecute(null!);
    }

    public bool CanExecute(object parameter)
    {
        return canExecute == null || canExecute();
    }

    public void Execute()
    {
        Execute(null!);
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