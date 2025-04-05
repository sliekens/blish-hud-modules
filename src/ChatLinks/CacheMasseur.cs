using Microsoft.Extensions.Caching.Memory;

namespace SL.ChatLinks;

public sealed class CacheMasseur<TItem>(IMemoryCache cache, string cacheKey) : IDisposable
{
    private readonly SemaphoreSlim _writeSemaphore = new(1, 1);

    public async ValueTask<TItem> GetOrCreate(Func<ICacheEntry, CancellationToken, ValueTask> factory,
        CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(cacheKey, out TItem found))
        {
            return found;
        }

        await _writeSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (cache.TryGetValue(cacheKey, out found))
            {
                return found;
            }

            return await CreateWithWriteLockAsync(factory, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _writeSemaphore.Release();
        }
    }

    public async Task<TItem> CreateAsync(Func<ICacheEntry, CancellationToken, ValueTask> factory,
        CancellationToken cancellationToken)
    {
        await _writeSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        return await CreateWithWriteLockAsync(factory, cancellationToken);
    }

    private async Task<TItem> CreateWithWriteLockAsync(Func<ICacheEntry, CancellationToken, ValueTask> factory, CancellationToken cancellationToken)
    {
        using ICacheEntry? entry = cache.CreateEntry(cacheKey);
        await factory(entry, cancellationToken).ConfigureAwait(false);
        return (TItem)entry.Value;
    }

    public void Dispose()
    {
        Clear();
        _writeSemaphore.Dispose();
    }

    public void Clear()
    {
        _writeSemaphore.Wait();
        try
        {
            cache.Remove(cacheKey);
        }
        finally
        {
            _writeSemaphore.Release();
        }
    }
}
