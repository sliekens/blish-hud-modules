using System.Collections.Concurrent;

using Blish_HUD.Content;

namespace SL.ChatLinks.UI;

public sealed class IconsCache : IDisposable
{
    private readonly ConcurrentDictionary<Uri, AsyncTexture2D> _webCache = [];

    public AsyncTexture2D GetOrAdd(Uri key, Func<Uri, AsyncTexture2D> valueFactory)
    {
        return _webCache.GetOrAdd(key, valueFactory);
    }

    public void Dispose()
    {
        List<KeyValuePair<Uri, AsyncTexture2D>> values = [.. _webCache];
        foreach (KeyValuePair<Uri, AsyncTexture2D> pair in values)
        {
            pair.Value.Dispose();
        }

        _webCache.Clear();
    }
}
