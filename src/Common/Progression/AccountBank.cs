using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Hero.Banking;
using GuildWars2.Hero.Inventories;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using SL.Common.Caching;

namespace SL.Common.Progression;

public sealed class AccountBank(
    ILogger<AccountBank> logger,
    ITokenProvider tokenProvider,
    Gw2Client gw2Client,
    IMemoryCache memoryCache
) : IDisposable
{
    private readonly CacheMasseur<IReadOnlyList<ItemSlot?>> _bank = new(memoryCache, "bank");

    public async ValueTask<IReadOnlyList<ItemSlot?>> GetBank(CancellationToken cancellationToken)
    {
        try
        {
            return tokenProvider.Grants.Contains(Permission.Inventories)
                ? await _bank.GetOrCreate(CacheBank, cancellationToken).ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Failed to retrieve account bank.");
            return [];
        }
    }

    public async Task Validate(bool force, CancellationToken cancellationToken)
    {
        if (tokenProvider.Grants.Contains(Permission.Inventories))
        {
            if (force || !_bank.TryGetValue(out _))
            {
                _ = await _bank.CreateAsync(CacheBank, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        else if (force)
        {
            ClearCache();
        }
    }

    private async ValueTask CacheBank(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (Bank value, MessageContext context) = await gw2Client.Hero.Bank
            .GetBank(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = value.Items;
    }

    public void ClearCache()
    {
        _bank.Clear();
    }

    public void Dispose()
    {
        _bank.Dispose();
    }
}
