using System.Windows.Input;

namespace SL.Common.ModelBinding;

public sealed class RelayCommand(Action execute) : ICommand
{
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool> canExecute)
        : this(execute)
    {
        _canExecute = canExecute;
    }

    public RelayCommand(
        Action execute,
        Func<bool> canExecute,
        Action<EventHandler> subscribeCanExecuteChanged,
        Action<EventHandler> unsubscribeCanExecuteChanged
    ) : this(execute, canExecute)
    {
        subscribeCanExecuteChanged.Invoke((sender, args) =>
        {
            CanExecuteChanged?.Invoke(sender, args);
        });

        unsubscribeCanExecuteChanged.Invoke((sender, args) =>
        {
            CanExecuteChanged?.Invoke(sender, args);
        });
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute()
    {
        return _canExecute == null || _canExecute();
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

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}