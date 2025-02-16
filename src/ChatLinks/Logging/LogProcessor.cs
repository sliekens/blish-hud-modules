using System.Collections.Concurrent;

namespace SL.ChatLinks.Logging;

public static class LogProcessor
{
    private static readonly ConcurrentQueue<Action> Work = new();

    private static readonly SemaphoreSlim QueueSemaphore = new(0);

    static LogProcessor()
    {
        _ = Task.Factory.StartNew(
            ProcessQueue,
            TaskCreationOptions.LongRunning
        );
    }

    public static void Enqueue(Action callback)
    {
        Work.Enqueue(callback);
        QueueSemaphore.Release();
    }

    private static async Task ProcessQueue()
    {
        while (true)
        {
            await QueueSemaphore.WaitAsync();

            while (Work.TryDequeue(out var work))
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
