using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Hero.Equipment.Gliders;

namespace SL.ChatLinks;

public sealed partial class Hero
{
    private IReadOnlyList<GliderSkin>? _gliderSkins;

    private IReadOnlyList<int>? _unlockedGliderSkins;

    public async ValueTask<IReadOnlyList<GliderSkin>> GetGliderSkins(CancellationToken cancellationToken)
    {
        return _gliderSkins ??= await GetGliderSkinsInternal(cancellationToken);
    }

    private async ValueTask<IReadOnlyList<GliderSkin>> GetGliderSkinsInternal(CancellationToken cancellationToken)
    {
        var gliderSkins = await _gw2Client.Hero.Equipment.Gliders
            .GetGliderSkins(cancellationToken: cancellationToken)
            .ValueOnly();
        return gliderSkins.ToImmutableList();
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedGliderSkins(CancellationToken cancellationToken)
    {
        return _unlockedGliderSkins ??= await GetUnlockedGliderSkinsInternal(cancellationToken);
    }

    private async ValueTask<IReadOnlyList<int>> GetUnlockedGliderSkinsInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            return [];
        }

        var token = await _tokenProvider.GetTokenAsync(cancellationToken);
        var values = await _gw2Client.Hero.Equipment.Gliders
            .GetUnlockedGliderSkins(token, cancellationToken: cancellationToken)
            .ValueOnly();

        return values.ToImmutableList();
    }
}