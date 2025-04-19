using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using SL.Common.Caching;

namespace SL.Common.Progression;

public sealed class UnlockedJadeBotSkins(
    ILogger<UnlockedJadeBotSkins> logger,
    ITokenProvider tokenProvider,
    Gw2Client gw2Client,
    IMemoryCache memoryCache
) : IDisposable
{
    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedJadeBotSkins = new(memoryCache, "unlocked_jade_bot_skins");

    public async ValueTask<IReadOnlyList<int>> GetUnlockedJadeBotSkins(CancellationToken cancellationToken)
    {
        try
        {
            return tokenProvider.Grants.Contains(Permission.Unlocks) && tokenProvider.Grants.Contains(Permission.Inventories)
                ? await _unlockedJadeBotSkins.GetOrCreate(CacheUnlockedJadeBotSkins, cancellationToken).ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Failed to retrieve unlocked jade bots.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedJadeBotSkins(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<int> values, MessageContext context) = await gw2Client.Hero.Equipment.JadeBots
            .GetUnlockedJadeBotSkins(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    public async Task Validate(bool force, CancellationToken cancellationToken)
    {
        if (tokenProvider.Grants.Contains(Permission.Unlocks) && tokenProvider.Grants.Contains(Permission.Inventories))
        {
            if (force || !_unlockedJadeBotSkins.TryGetValue(out _))
            {
                _ = await _unlockedJadeBotSkins.CreateAsync(CacheUnlockedJadeBotSkins, cancellationToken)
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
        _unlockedJadeBotSkins.Clear();
    }

    public void Dispose()
    {
        _unlockedJadeBotSkins.Dispose();
    }
}
