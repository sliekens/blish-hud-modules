using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Collections;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using SL.Common.Caching;

namespace SL.Common.Progression;

public sealed class UnlockedSkiffSkins(
    ILogger<UnlockedSkiffSkins> logger,
    ITokenProvider tokenProvider,
    Gw2Client gw2Client,
    IMemoryCache memoryCache
) : IDisposable
{
    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedSkiffSkins = new(memoryCache, "unlocked_skiff_skins");

    public async ValueTask<IReadOnlyList<int>> GetUnlockedSkiffSkins(CancellationToken cancellationToken)
    {
        try
        {
            return tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedSkiffSkins.GetOrCreate(CacheUnlockedSkiffSkins, cancellationToken).ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Failed to retrieve unlocked skiff skins.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedSkiffSkins(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (IImmutableValueSet<int> values, MessageContext context) = await gw2Client.Hero.Equipment.Skiffs
            .GetUnlockedSkiffSkins(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    public async Task Validate(bool force, CancellationToken cancellationToken)
    {
        if (tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            if (force || !_unlockedSkiffSkins.TryGetValue(out _))
            {
                _ = await _unlockedSkiffSkins.CreateAsync(CacheUnlockedSkiffSkins, cancellationToken)
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
        _unlockedSkiffSkins.Clear();
    }

    public void Dispose()
    {
        _unlockedSkiffSkins.Dispose();
    }
}
