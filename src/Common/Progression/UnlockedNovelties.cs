using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using SL.Common.Caching;

namespace SL.Common.Progression;

public sealed class UnlockedNovelties(
    ILogger<UnlockedNovelties> logger,
    ITokenProvider tokenProvider,
    Gw2Client gw2Client,
    IMemoryCache memoryCache
) : IDisposable
{
    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedNovelties = new(memoryCache, "unlocked_novelties");

    public async ValueTask<IReadOnlyList<int>> GetUnlockedNovelties(CancellationToken cancellationToken)
    {
        try
        {
            return tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedNovelties.GetOrCreate(CacheUnlockedNovelties, cancellationToken).ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Failed to retrieve unlocked novelties.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedNovelties(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<int> values, MessageContext context) = await gw2Client.Hero.Equipment.Novelties
            .GetUnlockedNovelties(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    public async Task Validate(bool force, CancellationToken cancellationToken)
    {
        if (tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            if (force || !_unlockedNovelties.TryGetValue(out _))
            {
                await _unlockedNovelties.CreateAsync(CacheUnlockedNovelties, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        else if (force)
        {
            ClearCache();
        }
    }

    public void ClearCache()
    {
        _unlockedNovelties.Clear();
    }

    public void Dispose()
    {
        _unlockedNovelties.Dispose();
    }
}
