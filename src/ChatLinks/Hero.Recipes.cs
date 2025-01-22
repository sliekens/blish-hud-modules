using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Hero.Crafting.Recipes;

namespace SL.ChatLinks;

public sealed partial class Hero
{
    private IReadOnlyList<Recipe>? _recipes;

    private IReadOnlyList<int>? _unlockedRecipes;

    public async ValueTask<IReadOnlyList<Recipe>> GetRecipes(CancellationToken cancellationToken)
    {
        return _recipes ??= await GetRecipesInternal(cancellationToken);
    }

    private async ValueTask<IReadOnlyList<Recipe>> GetRecipesInternal(CancellationToken cancellationToken)
    {
        List<Recipe>? recipes = null;
        await foreach ((Recipe skin, MessageContext context) in _gw2Client.Hero.Crafting.Recipes.GetRecipesBulk(cancellationToken: cancellationToken))
        {
            recipes ??= new List<Recipe>(context.ResultTotal.GetValueOrDefault());
            recipes.Add(skin);
        }

        return recipes.ToImmutableList();
    }

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