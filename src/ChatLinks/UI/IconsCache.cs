using System.Collections.Concurrent;

using Blish_HUD.Content;

namespace SL.ChatLinks.UI;

public sealed class IconsCache : IDisposable
{
    private readonly ConcurrentDictionary<string, AsyncTexture2D> _webCache = [];

    public AsyncTexture2D GetOrAdd(string key, Func<string, AsyncTexture2D> valueFactory)
    {
        return _webCache.GetOrAdd(key, valueFactory);
    }

    public void Dispose()
    {
        List<KeyValuePair<string, AsyncTexture2D>> values = [.. _webCache];
        foreach (KeyValuePair<string, AsyncTexture2D> pair in values)
        {
            pair.Value.Dispose();
        }

        _webCache.Clear();
    }
}
