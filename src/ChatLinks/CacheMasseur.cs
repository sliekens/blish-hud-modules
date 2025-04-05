using Microsoft.Extensions.Caching.Memory;

namespace SL.ChatLinks;

public sealed class CacheMasseur<TItem>(IMemoryCache cache, string cacheKey) : IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new();

    public async ValueTask<TItem> GetOrCreate(Func<ICacheEntry, CancellationToken, ValueTask> factory,
        CancellationToken cancellationToken)
    {
        _lock.EnterUpgradeableReadLock();
        try
        {
            if (cache.TryGetValue(cacheKey, out TItem found))
            {
                return found;
            }

            return await CreateAsync(factory, cancellationToken).ConfigureAwait(true);
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }

    public async Task<TItem> CreateAsync(Func<ICacheEntry, CancellationToken, ValueTask> factory,
        CancellationToken cancellationToken)
    {
        _lock.EnterWriteLock();
        try
        {
            using ICacheEntry? entry = cache.CreateEntry(cacheKey);
            await factory(entry, cancellationToken).ConfigureAwait(true);
            return (TItem)entry.Value;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void Dispose()
    {
        try
        {
            Clear();
        }
        finally
        {
            _lock.Dispose();
        }
    }

    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            cache.Remove(cacheKey);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}
