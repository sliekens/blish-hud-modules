using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Pvp.MistChampions;

namespace SL.ChatLinks;

public sealed partial class Hero
{
    private IReadOnlyList<MistChampionSkin>? _mistChampionSkins;

    private IReadOnlyList<int>? _unlockedMistChampionSkins;

    public async ValueTask<IReadOnlyList<MistChampionSkin>> GetMistChampionSkins(CancellationToken cancellationToken)
    {
        return _mistChampionSkins ??= await GetMistChampionSkinsInternal(cancellationToken);
    }

    private async ValueTask<IReadOnlyList<MistChampionSkin>> GetMistChampionSkinsInternal(CancellationToken cancellationToken)
    {
        var mistChampions = await _gw2Client.Pvp.GetMistChampions(cancellationToken: cancellationToken)
            .ValueOnly();
        return mistChampions.SelectMany(mistChampion => mistChampion.Skins).ToImmutableList();
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedMistChampionSkins(CancellationToken cancellationToken)
    {
        return _unlockedMistChampionSkins ??= await GetUnlockedMistChampionSkinsInternal(cancellationToken);
    }

    private async ValueTask<IReadOnlyList<int>> GetUnlockedMistChampionSkinsInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            return [];
        }

        var token = await _tokenProvider.GetTokenAsync(cancellationToken);
        var values = await _gw2Client.Pvp
            .GetUnlockedMistChampions(token, cancellationToken: cancellationToken)
            .ValueOnly();

        return values.ToImmutableList();
    }
}