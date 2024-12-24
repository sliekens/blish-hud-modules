using Blish_HUD;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SL.ChatLinks.Logging;

public class LoggingAdapter<T>(string categoryName, IOptionsMonitor<LoggerFilterOptions> options) : ILogger
{
    private static readonly Logger Sink = Logger.GetLogger<T>();

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        string logMessage = $"{categoryName} | {formatter(state, exception)}";
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
        // Check if global filters disable this log level
        foreach (var rule in options.CurrentValue.Rules)
        {
            if (rule.ProviderName is not null or "Blish_HUD")
            {
                continue;
            }

            // Match category-specific or fallback rules
            if (rule.CategoryName == null || categoryName.StartsWith(rule.CategoryName, StringComparison.OrdinalIgnoreCase))
            {
                if (rule.LogLevel == null || logLevel >= rule.LogLevel)
                {
                    return rule.Filter?.Invoke("Blish_HUD", categoryName, logLevel) ?? true;
                }
            }
        }

        // Default to the global minimum log level if no rules match
        return logLevel >= options.CurrentValue.MinLevel;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }
}
