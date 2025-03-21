using System.Windows.Input;

namespace SL.Common.ModelBinding;

public sealed class AsyncRelayCommand<T>(Func<T, Task> execute) : ICommand
{
    private readonly Func<T, bool>? _canExecute;

    public AsyncRelayCommand(Func<T, Task> execute, Func<T, bool> canExecute)
        : this(execute)
    {
        _canExecute = canExecute;
    }

    public AsyncRelayCommand(
        Func<T, Task> execute,
        Func<T, bool> canExecute,
        Action<EventHandler> subscribeCanExecuteChanged,
        Action<EventHandler> unsubscribeCanExecuteChanged
    ) : this(execute, canExecute)
    {
        ThrowHelper.ThrowIfNull(subscribeCanExecuteChanged);
        ThrowHelper.ThrowIfNull(unsubscribeCanExecuteChanged);
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

    bool ICommand.CanExecute(object parameter)
    {
        return CanExecute((T)parameter);
    }

    public bool CanExecute(T parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    void ICommand.Execute(object parameter)
    {
        Execute((T)parameter);
    }

    public void Execute(T parameter)
    {
        _ = execute(parameter);
    }

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
