using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Logging;

namespace SL.ChatLinks;

public sealed partial class Hero
{
    private IReadOnlyList<int>? _unlockedRecipes;

    public async ValueTask<IReadOnlyList<int>> GetUnlockedRecipes(CancellationToken cancellationToken)
    {
        try
        {
            return _unlockedRecipes ??= await GetUnlockedRecipesInternal(cancellationToken);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked recipes.");
            return [];
        }
    }

    private async ValueTask<IReadOnlyList<int>> GetUnlockedRecipesInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            return [];
        }

        var token = await _tokenProvider.GetTokenAsync(cancellationToken);
        var values = await _gw2Client.Hero.Crafting.Recipes
            .GetUnlockedRecipes(token, cancellationToken: cancellationToken)
            .ValueOnly();

        return [.. values];
    }
}
