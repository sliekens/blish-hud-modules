namespace SL.Common;

/// <summary>
/// The EventAggregator class is used to manage events in an MVVM architecture.
/// It allows different parts of the application to communicate with each other without having direct references to each other, promoting loose coupling.
/// </summary>
public interface IEventAggregator
{
    /// <summary>
    /// Subscribes a synchronous handler to an event of type TEvent.
    /// </summary>
    void Subscribe<TEvent>(Action<TEvent> syncHandler);

    /// <summary>
    /// Subscribes an asynchronous handler to an event of type TEvent.
    /// </summary>
    void Subscribe<TEvent>(Func<TEvent, ValueTask> asyncHandler);

    /// <summary>
    /// Unsubscribes a specific handler from an event of type TEvent.
    /// </summary>
    void Unsubscribe<TEvent>(Delegate handler);

    /// <summary>
    /// Publishes an event of type TEvent to all subscribers synchronously.
    /// </summary>
    void Publish<TEvent>(TEvent eventToPublish);

    /// <summary>
    /// Publishes an event of type TEvent to all subscribers asynchronously.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent eventToPublish);

    /// <summary>
    /// Publishes an event of type TEvent to all subscribers asynchronously.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent eventToPublish, CancellationToken cancellationToken);
}
