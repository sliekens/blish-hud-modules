using Microsoft.Extensions.Caching.Memory;

namespace SL.Common.Caching;

public sealed class CacheMasseur<TItem>(IMemoryCache cache, string cacheKey) : IDisposable
{
    private readonly SemaphoreSlim _writeSemaphore = new(1, 1);

    public bool TryGetValue(out TItem value) => cache.TryGetValue(cacheKey, out value);

    public async ValueTask<TItem> GetOrCreate(Func<ICacheEntry, CancellationToken, ValueTask> factory,
        CancellationToken cancellationToken)
    {
        ThrowHelper.ThrowIfNull(factory);
        if (cache.TryGetValue(cacheKey, out TItem found))
        {
            return found;
        }

        await _writeSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return cache.TryGetValue(cacheKey, out found)
                ? found
                : await CreateWithWriteLockAsync(factory, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _ = _writeSemaphore.Release();
        }
    }

    public async Task<TItem> CreateAsync(Func<ICacheEntry, CancellationToken, ValueTask> factory,
        CancellationToken cancellationToken)
    {
        ThrowHelper.ThrowIfNull(factory);
        await _writeSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await CreateWithWriteLockAsync(factory, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _ = _writeSemaphore.Release();
        }
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
            _ = _writeSemaphore.Release();
        }
    }
}
