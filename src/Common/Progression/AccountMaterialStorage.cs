using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Hero.Banking;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using SL.Common.Caching;

namespace SL.Common.Progression;

public sealed class AccountMaterialStorage(
    ILogger<AccountMaterialStorage> logger,
    ITokenProvider tokenProvider,
    Gw2Client gw2Client,
    IMemoryCache memoryCache
) : IDisposable
{
    private readonly CacheMasseur<IReadOnlyList<MaterialSlot>> _materialStorage = new(memoryCache, "material_storage");

    public async ValueTask<IReadOnlyList<MaterialSlot>> GetMaterialStorage(CancellationToken cancellationToken)
    {
        try
        {
            return tokenProvider.Grants.Contains(Permission.Inventories)
                ? await _materialStorage.GetOrCreate(CacheMaterialStorage, cancellationToken).ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Failed to retrieve material storage.");
            return [];
        }
    }

    public async Task Validate(bool force, CancellationToken cancellationToken)
    {
        if (tokenProvider.Grants.Contains(Permission.Inventories))
        {
            if (force || !_materialStorage.TryGetValue(out _))
            {
                _ = await _materialStorage.CreateAsync(CacheMaterialStorage, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        else if (force)
        {
            ClearCache();
        }
    }

    private async ValueTask CacheMaterialStorage(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (MaterialStorage value, MessageContext context) = await gw2Client.Hero.Bank
            .GetMaterialStorage(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = value.Materials;
    }

    public void ClearCache()
    {
        _materialStorage.Clear();
    }

    public void Dispose()
    {
        _materialStorage.Dispose();
    }
}
