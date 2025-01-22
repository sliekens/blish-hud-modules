using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Hero.Equipment.JadeBots;

namespace SL.ChatLinks;

public sealed partial class Hero
{
    private IReadOnlyList<JadeBotSkin>? _jadeBotSkins;

    private IReadOnlyList<int>? _unlockedJadeBotSkins;

    public async ValueTask<IReadOnlyList<JadeBotSkin>> GetJadeBotSkins(CancellationToken cancellationToken)
    {
        return _jadeBotSkins ??= await GetJadeBotSkinsInternal(cancellationToken);
    }

    private async ValueTask<IReadOnlyList<JadeBotSkin>> GetJadeBotSkinsInternal(CancellationToken cancellationToken)
    {
        var jadeBotSkins = await _gw2Client.Hero.Equipment.JadeBots
            .GetJadeBotSkins(cancellationToken: cancellationToken)
            .ValueOnly();
        return jadeBotSkins.ToImmutableList();
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedJadeBotSkins(CancellationToken cancellationToken)
    {
        return _unlockedJadeBotSkins ??= await GetUnlockedJadeBotSkinsInternal(cancellationToken);
    }

    private async ValueTask<IReadOnlyList<int>> GetUnlockedJadeBotSkinsInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Unlocks)
            || !_tokenProvider.Grants.Contains(Permission.Inventories))
        {
            return [];
        }

        var token = await _tokenProvider.GetTokenAsync(cancellationToken);
        var values = await _gw2Client.Hero.Equipment.JadeBots
            .GetUnlockedJadeBotSkins(token, cancellationToken: cancellationToken)
            .ValueOnly();

        return values.ToImmutableList();
    }
}