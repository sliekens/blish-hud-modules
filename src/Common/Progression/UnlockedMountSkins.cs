using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Collections;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using SL.Common.Caching;

namespace SL.Common.Progression;

public sealed class UnlockedMountSkins(
    ILogger<UnlockedMountSkins> logger,
    ITokenProvider tokenProvider,
    Gw2Client gw2Client,
    IMemoryCache memoryCache
) : IDisposable
{
    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedMountSkins = new(memoryCache, "unlocked_mount_skins");

    public async ValueTask<IReadOnlyList<int>> GetUnlockedMountSkins(CancellationToken cancellationToken)
    {
        try
        {
            return tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedMountSkins.GetOrCreate(CacheUnlockedMountSkins, cancellationToken).ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Failed to retrieve unlocked mount skins.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedMountSkins(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (IImmutableValueSet<int> values, MessageContext context) = await gw2Client.Hero.Equipment.Mounts
            .GetUnlockedMountSkins(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    public async Task Validate(bool force, CancellationToken cancellationToken)
    {
        if (tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            if (force || !_unlockedMountSkins.TryGetValue(out _))
            {
                _ = await _unlockedMountSkins.CreateAsync(CacheUnlockedMountSkins, cancellationToken)
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
        _unlockedMountSkins.Clear();
    }

    public void Dispose()
    {
        _unlockedMountSkins.Dispose();
    }
}
