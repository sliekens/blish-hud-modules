using System.Collections.Concurrent;

using Blish_HUD.Content;

namespace SL.ChatLinks.UI;

public sealed class IconsCache : IDisposable
{
    private readonly ConcurrentDictionary<string, AsyncTexture2D> WebCache = [];

    public AsyncTexture2D GetOrAdd(string key, Func<string, AsyncTexture2D> valueFactory)
    {
        return WebCache.GetOrAdd(key, valueFactory);
    }

    public void Dispose()
    {
        List<KeyValuePair<string, AsyncTexture2D>> values = WebCache.ToList();
        foreach (KeyValuePair<string, AsyncTexture2D> pair in values)
        {
            pair.Value.Dispose();
        }

        WebCache.Clear();
    }
}
