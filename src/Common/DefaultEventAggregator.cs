using System.Collections.Concurrent;

namespace SL.Common;

public sealed class DefaultEventAggregator : IEventAggregator
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _eventHandlers = new();

    public void Subscribe<TEvent>(Action<TEvent> syncHandler)
    {
        var handlers = _eventHandlers.GetOrAdd(typeof(TEvent), _ => []);
        lock (handlers)
        {
            handlers.Add(syncHandler);
        }
    }

    public void Subscribe<TEvent>(Func<TEvent, ValueTask> asyncHandler)
    {
        var handlers = _eventHandlers.GetOrAdd(typeof(TEvent), _ => []);
        lock (handlers)
        {
            handlers.Add(asyncHandler);
        }
    }


    public void Unsubscribe<TEvent>(Delegate handler)
    {
        if (!_eventHandlers.TryGetValue(typeof(TEvent), out var handlers))
        {
            return;
        }

        lock (handlers)
        {
            handlers.Remove(handler);

            if (handlers.Count == 0)
            {
                _eventHandlers.TryRemove(typeof(TEvent), out _);
            }
        }
    }

    public void Publish<TEvent>(TEvent eventToPublish)
    {
        if (!_eventHandlers.TryGetValue(typeof(TEvent), out var handlers))
        {
            return;
        }

        List<Delegate>? copy;
        lock (handlers)
        {
            copy = [.. handlers];
        }

        foreach (var handler in copy)
        {
            switch (handler)
            {
                case Action<TEvent> syncHandler:
                    syncHandler(eventToPublish);
                    break;
                case Func<TEvent, Task> asyncHandler:
                    _ = asyncHandler(eventToPublish);
                    break;
            }
        }
    }

    public async Task PublishAsync<TEvent>(TEvent eventToPublish, CancellationToken cancellationToken)
    {
        if (!_eventHandlers.TryGetValue(typeof(TEvent), out var handlers))
        {
            return;
        }

        List<Delegate>? copy;
        lock (handlers)
        {
            copy = [.. handlers];
        }

        var tasks = new List<Task>();
        foreach (var handler in copy)
        {
            switch (handler)
            {
                case Action<TEvent> syncHandler:
                    cancellationToken.ThrowIfCancellationRequested();
                    syncHandler(eventToPublish);
                    break;
                case Func<TEvent, ValueTask> asyncHandler:
                    cancellationToken.ThrowIfCancellationRequested();
                    tasks.Add(asyncHandler(eventToPublish).AsTask());
                    break;
            }
        }

        if (tasks.Count > 0)
        {
            await WhenAllWithCancellation(tasks, cancellationToken);
        }
    }

    private static async Task WhenAllWithCancellation(IEnumerable<Task> tasks, CancellationToken cancellationToken)
    {
        var taskList = tasks.ToList();
        var completionSource = new TaskCompletionSource<object>();

        using (cancellationToken.Register(() => completionSource.TrySetCanceled()))
        {
            var allTasks = Task.WhenAny(Task.WhenAll(taskList), completionSource.Task);

            var completedTask = await allTasks.ConfigureAwait(false);

            if (completedTask == completionSource.Task)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            await Task.WhenAll(taskList).ConfigureAwait(false);
        }
    }
}