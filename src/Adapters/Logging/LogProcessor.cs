using System.Collections.Concurrent;

namespace SL.Adapters.Logging;

public static class LogProcessor
{
    private static readonly ConcurrentQueue<Action> Work = new();

    private static readonly SemaphoreSlim QueueSemaphore = new(0);

    static LogProcessor()
    {
        _ = Task.Factory.StartNew(
            ProcessQueue,
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
        );
    }

    public static void Enqueue(Action callback)
    {
        Work.Enqueue(callback);
        _ = QueueSemaphore.Release();
    }

    private static async Task ProcessQueue()
    {
        while (true)
        {
            await QueueSemaphore.WaitAsync().ConfigureAwait(false);

            while (Work.TryDequeue(out Action? work))
            {
                try
                {
                    work?.Invoke();
                }
                catch
                {
                    // too bad
                }
            }
        }
    }

}
