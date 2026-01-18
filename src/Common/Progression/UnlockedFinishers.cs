using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Collections;
using GuildWars2.Hero.Equipment.Finishers;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using SL.Common.Caching;

namespace SL.Common.Progression;

public sealed class UnlockedFinishers(
    ILogger<UnlockedFinishers> logger,
    ITokenProvider tokenProvider,
    Gw2Client gw2Client,
    IMemoryCache memoryCache
) : IDisposable
{
    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedFinishers = new(memoryCache, "unlocked_finishers");

    public async ValueTask<IReadOnlyList<int>> GetUnlockedFinishers(CancellationToken cancellationToken)
    {
        try
        {
            return tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedFinishers.GetOrCreate(CacheUnlockedFinishers, cancellationToken).ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Failed to retrieve unlocked finishers.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedFinishers(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (IImmutableValueSet<UnlockedFinisher> values, MessageContext context) = await gw2Client.Hero.Equipment.Finishers
            .GetUnlockedFinishers(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.Select(finisher => finisher.Id).ToImmutableArray();
    }

    public async Task Validate(bool force, CancellationToken cancellationToken)
    {
        if (tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            if (force || !_unlockedFinishers.TryGetValue(out _))
            {
                _ = await _unlockedFinishers.CreateAsync(CacheUnlockedFinishers, cancellationToken)
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
        _unlockedFinishers.Clear();
    }

    public void Dispose()
    {
        _unlockedFinishers.Dispose();
    }
}
