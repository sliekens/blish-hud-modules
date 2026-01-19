using Blish_HUD;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SL.Adapters.Logging;

public class LoggingAdapter<T>(string categoryName, IOptionsMonitor<LoggerFilterOptions> options) : ILogger
{
    private readonly Logger _sink = Logger.GetLogger<T>();

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        LogProcessor.Enqueue(() =>
        {
            string logMessage = $"{categoryName}: {formatter(state, exception)}";
            switch (logLevel)
            {
                case LogLevel.Trace:
                    _sink.Trace(exception, logMessage);
                    break;
                case LogLevel.Debug:
                    _sink.Debug(exception, logMessage);
                    break;
                case LogLevel.Information:
                    _sink.Info(exception, logMessage);
                    break;
                case LogLevel.Warning:
                    _sink.Warn(exception, logMessage);
                    break;
                case LogLevel.Error:
                    _sink.Error(exception, logMessage);
                    break;
                case LogLevel.Critical:
                    _sink.Fatal(exception, logMessage);
                    break;
                case LogLevel.None:
                    break;
                default:
                    break;
            }
        });
    }


    public bool IsEnabled(LogLevel logLevel)
    {
        // Check if global filters disable this log level
        foreach (LoggerFilterRule? rule in options.CurrentValue.Rules)
        {
            if (rule.ProviderName is not null and not "Blish_HUD")
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
