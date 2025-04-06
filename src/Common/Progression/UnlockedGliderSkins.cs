using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using SL.Common.Caching;

namespace SL.Common.Progression;

public sealed class UnlockedGliderSkins(
    ILogger<UnlockedGliderSkins> logger,
    ITokenProvider tokenProvider,
    Gw2Client gw2Client,
    IMemoryCache memoryCache
) : IDisposable
{
    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedGliderSkins = new(memoryCache, "unlocked_glider_skins");

    public async ValueTask<IReadOnlyList<int>> GetUnlockedGliderSkins(CancellationToken cancellationToken)
    {
        try
        {
            return tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedGliderSkins.GetOrCreate(CacheUnlockedGliderSkins, cancellationToken).ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Failed to retrieve unlocked glider skins.");
            return [];
        }
    }

    public async Task Validate(bool force, CancellationToken cancellationToken)
    {
        if (tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            if (force || !_unlockedGliderSkins.TryGetValue(out _))
            {
                await _unlockedGliderSkins.CreateAsync(CacheUnlockedGliderSkins, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        else if (force)
        {
            ClearCache();
        }
    }

    private async ValueTask CacheUnlockedGliderSkins(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<int> values, MessageContext context) = await gw2Client.Hero.Equipment.Gliders
            .GetUnlockedGliderSkins(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    public void ClearCache()
    {
        _unlockedGliderSkins.Clear();
    }

    public void Dispose()
    {
        _unlockedGliderSkins.Dispose();
    }
}
