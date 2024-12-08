using Microsoft.Extensions.Logging;

namespace SL.ChatLinks.Logging;

public class LoggingAdapterProvider<T> : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new LoggingAdapter<T>();
    }

    public void Dispose()
    {
    }
}