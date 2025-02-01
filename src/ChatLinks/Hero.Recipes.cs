using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

namespace SL.ChatLinks;

public sealed partial class Hero
{
    private IReadOnlyList<int>? _unlockedRecipes;

    public async ValueTask<IReadOnlyList<int>> GetUnlockedRecipes(CancellationToken cancellationToken)
    {
        return _unlockedRecipes ??= await GetUnlockedRecipesInternal(cancellationToken);
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

        return values.ToImmutableList();
    }
}