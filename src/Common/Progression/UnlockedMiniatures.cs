using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using SL.Common.Caching;

namespace SL.Common.Progression;

public sealed class UnlockedMiniatures(
    ILogger<UnlockedMiniatures> logger,
    ITokenProvider tokenProvider,
    Gw2Client gw2Client,
    IMemoryCache memoryCache
) : IDisposable
{
    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedMiniatures = new(memoryCache, "unlocked_miniatures");

    public async ValueTask<IReadOnlyList<int>> GetUnlockedMiniatures(CancellationToken cancellationToken)
    {
        try
        {
            return tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedMiniatures.GetOrCreate(CacheUnlockedMiniatures, cancellationToken).ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Failed to retrieve unlocked miniatures.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedMiniatures(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<int> values, MessageContext context) = await gw2Client.Hero.Equipment.Miniatures
            .GetUnlockedMiniatures(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    public void ClearCache()
    {
        _unlockedMiniatures.Clear();
    }

    public void Dispose()
    {
        _unlockedMiniatures.Dispose();
    }
}
