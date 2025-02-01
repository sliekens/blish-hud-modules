using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

namespace SL.ChatLinks;

public sealed partial class Hero
{
    private IReadOnlyList<int>? _unlockedWardrobe;

    public async ValueTask<IReadOnlyList<int>> GetUnlockedWardrobe(CancellationToken cancellationToken)
    {
        return _unlockedWardrobe ??= await GetUnlockedWardrobeInternal(cancellationToken);
    }

    private async ValueTask<IReadOnlyList<int>> GetUnlockedWardrobeInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            return [];
        }

        var token = await _tokenProvider.GetTokenAsync(cancellationToken);
        var values = await _gw2Client.Hero.Equipment.Wardrobe
            .GetUnlockedSkins(token, cancellationToken)
            .ValueOnly();

        return values.ToImmutableList();
    }
}