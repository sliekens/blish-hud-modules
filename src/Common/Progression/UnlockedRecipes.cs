using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using SL.Common.Caching;

namespace SL.Common.Progression;

public sealed class UnlockedRecipes(
    ILogger<UnlockedRecipes> logger,
    ITokenProvider tokenProvider,
    Gw2Client gw2Client,
    IMemoryCache memoryCache
) : IDisposable
{
    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedRecipes = new(memoryCache, "unlocked_recipes");

    public async ValueTask<IReadOnlyList<int>> GetUnlockedRecipes(CancellationToken cancellationToken)
    {
        try
        {
            return tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedRecipes.GetOrCreate(CacheUnlockedRecipes, cancellationToken).ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Failed to retrieve unlocked recipes.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedRecipes(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<int> values, MessageContext context) = await gw2Client.Hero.Crafting.Recipes
            .GetUnlockedRecipes(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    public void ClearCache()
    {
        _unlockedRecipes.Clear();
    }

    public void Dispose()
    {
        _unlockedRecipes.Dispose();
    }
}
