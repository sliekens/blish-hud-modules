using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SL.ChatLinks.Logging;

public sealed class LoggingAdapterProvider<T>(IOptionsMonitor<LoggerFilterOptions> options) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new LoggingAdapter<T>(categoryName, options);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

