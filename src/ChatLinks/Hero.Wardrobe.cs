using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Hero.Equipment.Wardrobe;

namespace SL.ChatLinks;

public sealed partial class Hero
{
    private IReadOnlyList<EquipmentSkin>? _wardrobe;

    private IReadOnlyList<int>? _unlockedWardrobe;

    public async ValueTask<IReadOnlyList<EquipmentSkin>> GetWardrobe(CancellationToken cancellationToken)
    {
        return _wardrobe ??= await GetWardrobeInternal(cancellationToken);
    }

    private async ValueTask<IReadOnlyList<EquipmentSkin>> GetWardrobeInternal(CancellationToken cancellationToken)
    {
        List<EquipmentSkin>? skins = null;
        await foreach ((EquipmentSkin skin, MessageContext context) in _gw2Client.Hero.Equipment.Wardrobe
            .GetSkinsBulk(cancellationToken: cancellationToken))
        {
            skins ??= new List<EquipmentSkin>(context.ResultTotal.GetValueOrDefault());
            skins.Add(skin);
        }

        return skins.ToImmutableList();
    }

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