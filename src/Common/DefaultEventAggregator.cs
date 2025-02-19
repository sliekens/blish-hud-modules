using System.Collections.Concurrent;

namespace SL.Common;

public sealed class DefaultEventAggregator : IEventAggregator
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _eventHandlers = new();

    public void Subscribe<TEvent>(Action<TEvent> syncHandler)
    {
        List<Delegate> handlers = _eventHandlers.GetOrAdd(typeof(TEvent), _ => []);
        lock (handlers)
        {
            handlers.Add(syncHandler);
        }
    }

    public void Subscribe<TEvent>(Func<TEvent, Task> asyncHandler)
    {
        List<Delegate> handlers = _eventHandlers.GetOrAdd(typeof(TEvent), _ => []);
        lock (handlers)
        {
            handlers.Add(asyncHandler);
        }
    }


    public void Unsubscribe<TEvent>(Delegate handler)
    {
        if (!_eventHandlers.TryGetValue(typeof(TEvent), out List<Delegate>? handlers))
        {
            return;
        }

        lock (handlers)
        {
            _ = handlers.Remove(handler);

            if (handlers.Count == 0)
            {
                _ = _eventHandlers.TryRemove(typeof(TEvent), out _);
            }
        }
    }

    public void Publish<TEvent>(TEvent eventToPublish)
    {
        if (!_eventHandlers.TryGetValue(typeof(TEvent), out List<Delegate>? handlers))
        {
            return;
        }

        List<Delegate>? copy;
        lock (handlers)
        {
            copy = [.. handlers];
        }

        foreach (Delegate handler in copy)
        {
            switch (handler)
            {
                case Action<TEvent> syncHandler:
                    syncHandler(eventToPublish);
                    break;
                case Func<TEvent, Task> asyncHandler:
                    _ = asyncHandler(eventToPublish).ConfigureAwait(false);
                    break;
                default:
                    break;
            }
        }
    }

    public async Task PublishAsync<TEvent>(TEvent eventToPublish)
    {
        await PublishAsync(eventToPublish, CancellationToken.None)
            .ConfigureAwait(false);
    }

    public async Task PublishAsync<TEvent>(TEvent eventToPublish, CancellationToken cancellationToken)
    {
        if (!_eventHandlers.TryGetValue(typeof(TEvent), out List<Delegate>? handlers))
        {
            return;
        }

        List<Delegate>? copy;
        lock (handlers)
        {
            copy = [.. handlers];
        }

        List<Task> tasks = [];
        foreach (Delegate handler in copy)
        {
            switch (handler)
            {
                case Action<TEvent> syncHandler:
                    cancellationToken.ThrowIfCancellationRequested();
                    syncHandler(eventToPublish);
                    break;
                case Func<TEvent, Task> asyncHandler:
                    cancellationToken.ThrowIfCancellationRequested();
                    tasks.Add(asyncHandler(eventToPublish));
                    break;
                default:
                    break;
            }
        }

        if (tasks.Count > 0)
        {
            await WhenAllWithCancellation(tasks, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task WhenAllWithCancellation(IEnumerable<Task> tasks, CancellationToken cancellationToken)
    {
        List<Task> taskList = [.. tasks];
        TaskCompletionSource<object> completionSource = new();

        using (cancellationToken.Register(() => completionSource.TrySetCanceled()))
        {
            Task<Task> allTasks = Task.WhenAny(Task.WhenAll(taskList), completionSource.Task);

            Task completedTask = await allTasks.ConfigureAwait(false);

            if (completedTask == completionSource.Task)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            await Task.WhenAll(taskList).ConfigureAwait(false);
        }
    }
}
