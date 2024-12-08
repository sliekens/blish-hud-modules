using Blish_HUD;

using Microsoft.Extensions.Logging;

namespace SL.ChatLinks.Logging;

public class LoggingAdapter<T> : ILogger
{
    private static readonly Logger Sink = Logger.GetLogger<T>();

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        string logMessage = formatter(state, exception);
        switch (logLevel)
        {
            case LogLevel.Trace:
                Sink.Trace(exception, logMessage);
                break;
            case LogLevel.Debug:
                Sink.Debug(exception, logMessage);
                break;
            case LogLevel.Information:
                Sink.Info(exception, logMessage);
                break;
            case LogLevel.Warning:
                Sink.Warn(exception, logMessage);
                break;
            case LogLevel.Error:
                Sink.Error(exception, logMessage);
                break;
            case LogLevel.Critical:
                Sink.Fatal(exception, logMessage);
                break;
            case LogLevel.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }
}
