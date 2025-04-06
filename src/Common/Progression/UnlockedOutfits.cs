using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using SL.Common.Caching;

namespace SL.Common.Progression;

public sealed class UnlockedOutfits(
    ILogger<UnlockedOutfits> logger,
    ITokenProvider tokenProvider,
    Gw2Client gw2Client,
    IMemoryCache memoryCache
) : IDisposable
{
    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedOutfits = new(memoryCache, "unlocked_outfits");

    public async ValueTask<IReadOnlyList<int>> GetUnlockedOutfits(CancellationToken cancellationToken)
    {
        try
        {
            return tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedOutfits.GetOrCreate(CacheUnlockedOutfits, cancellationToken).ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Failed to retrieve unlocked outfits.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedOutfits(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<int> values, MessageContext context) = await gw2Client.Hero.Equipment.Outfits
            .GetUnlockedOutfits(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    public async Task Validate(bool force, CancellationToken cancellationToken)
    {
        if (tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            if (force || !_unlockedOutfits.TryGetValue(out _))
            {
                await _unlockedOutfits.CreateAsync(CacheUnlockedOutfits, cancellationToken)
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
        _unlockedOutfits.Clear();
    }

    public void Dispose()
    {
        _unlockedOutfits.Dispose();
    }
}
