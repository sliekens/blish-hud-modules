using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using SL.Common.Caching;

namespace SL.Common.Progression;

public sealed class UnlockedDyes(
    ILogger<UnlockedDyes> logger,
    ITokenProvider tokenProvider,
    Gw2Client gw2Client,
    IMemoryCache memoryCache
) : IDisposable
{
    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedDyes = new(memoryCache, "unlocked_dyes");

    public async ValueTask<IReadOnlyList<int>> GetUnlockedDyes(CancellationToken cancellationToken)
    {
        try
        {
            return tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedDyes.GetOrCreate(CacheUnlockedDyes, cancellationToken).ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Failed to retrieve unlocked dyes.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedDyes(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<int> values, MessageContext context) = await gw2Client.Hero.Equipment.Dyes
            .GetUnlockedColors(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    public async Task Validate(bool force, CancellationToken cancellationToken)
    {
        if (tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            if (force || !_unlockedDyes.TryGetValue(out _))
            {
                await _unlockedDyes.CreateAsync(CacheUnlockedDyes, cancellationToken)
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
        _unlockedDyes.Clear();
    }

    public void Dispose()
    {
        _unlockedDyes.Dispose();
    }
}
