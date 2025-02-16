using System.Windows.Input;

namespace SL.Common.ModelBinding;

public sealed class RelayCommand<T>(Action<T> execute) : ICommand
{
    private readonly Func<T, bool>? _canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
        : this(execute)
    {
        _canExecute = canExecute;
    }

    public RelayCommand(
        Action<T> execute,
        Func<T, bool> canExecute,
        Action<EventHandler> subscribeCanExecuteChanged,
        Action<EventHandler> unsubscribeCanExecuteChanged
    ) : this(execute, canExecute)
    {
        subscribeCanExecuteChanged.Invoke((sender, args) =>
        {
            CanExecuteChanged?.Invoke(sender, args);
        });

        unsubscribeCanExecuteChanged.Invoke(((sender, args) =>
        {
            CanExecuteChanged?.Invoke(sender, args);
        }));
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(T parameter)
    {
        return _canExecute == null || _canExecute(parameter);
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
