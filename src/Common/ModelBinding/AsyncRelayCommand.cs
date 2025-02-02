﻿using System.Windows.Input;

namespace SL.Common.ModelBinding;

public sealed class AsyncRelayCommand(Func<Task> execute) : ICommand
{
    private readonly Func<bool>? _canExecute;

    public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute)
        : this(execute)
    {
        _canExecute = canExecute;
    }

    public AsyncRelayCommand(
        Func<Task> execute,
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

    public void OnCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}