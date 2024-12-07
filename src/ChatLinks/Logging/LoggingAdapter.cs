using Blish_HUD;

using Microsoft.Extensions.Logging;

namespace SL.ChatLinks.Logging;

public class LoggingAdapterProvider<T> : ILoggerProvider
{
    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new LoggingAdapter<T>();
    }
}

public class LoggingAdapter<T> : ILogger
{
    private readonly Logger _logger = Logger.GetLogger<T>();

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        string logMessage = formatter(state, exception);
        switch (logLevel)
        {
            case LogLevel.Trace:
                _logger.Trace(exception, logMessage);
                break;
            case LogLevel.Debug:
                _logger.Debug(exception, logMessage);
                break;
            case LogLevel.Information:
                _logger.Info(exception, logMessage);
                break;
            case LogLevel.Warning:
                _logger.Warn(exception, logMessage);
                break;
            case LogLevel.Error:
                _logger.Error(exception, logMessage);
                break;
            case LogLevel.Critical:
                _logger.Fatal(exception, logMessage);
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
