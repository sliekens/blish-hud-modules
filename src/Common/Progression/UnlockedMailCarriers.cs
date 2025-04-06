using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using SL.Common.Caching;

namespace SL.Common.Progression;

public sealed class UnlockedMailCarriers(
    ILogger<UnlockedMailCarriers> logger,
    ITokenProvider tokenProvider,
    Gw2Client gw2Client,
    IMemoryCache memoryCache
) : IDisposable
{
    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedMailCarriers = new(memoryCache, "unlocked_mail_carriers");

    public async ValueTask<IReadOnlyList<int>> GetUnlockedMailCarriers(CancellationToken cancellationToken)
    {
        try
        {
            return tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedMailCarriers.GetOrCreate(CacheUnlockedMailCarriers, cancellationToken).ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Failed to retrieve unlocked mail carriers.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedMailCarriers(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<int> values, MessageContext context) = await gw2Client.Hero.Equipment.MailCarriers
            .GetUnlockedMailCarriers(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    public async Task Validate(bool force, CancellationToken cancellationToken)
    {
        if (tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            if (force || !_unlockedMailCarriers.TryGetValue(out _))
            {
                await _unlockedMailCarriers.CreateAsync(CacheUnlockedMailCarriers, cancellationToken)
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
        _unlockedMailCarriers.Clear();
    }

    public void Dispose()
    {
        _unlockedMailCarriers.Dispose();
    }
}
