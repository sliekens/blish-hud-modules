using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Hero.Equipment.Outfits;

namespace SL.ChatLinks;

public sealed partial class Hero
{
    private IReadOnlyList<Outfit>? _outfits;

    private IReadOnlyList<int>? _unlockedOutfits;

    public async ValueTask<IReadOnlyList<Outfit>> GetOutfits(CancellationToken cancellationToken)
    {
        return _outfits ??= await GetOutfitsInternal(cancellationToken);
    }

    private async ValueTask<IReadOnlyList<Outfit>> GetOutfitsInternal(CancellationToken cancellationToken)
    {
        var outfits = await _gw2Client.Hero.Equipment.Outfits
            .GetOutfits(cancellationToken: cancellationToken)
            .ValueOnly();
        return outfits.ToImmutableList();
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedOutfits(CancellationToken cancellationToken)
    {
        return _unlockedOutfits ??= await GetUnlockedOutfitsInternal(cancellationToken);
    }

    private async ValueTask<IReadOnlyList<int>> GetUnlockedOutfitsInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            return [];
        }

        var token = await _tokenProvider.GetTokenAsync(cancellationToken);
        var values = await _gw2Client.Hero.Equipment.Outfits
            .GetUnlockedOutfits(token, cancellationToken: cancellationToken)
            .ValueOnly();

        return values.ToImmutableList();
    }
}