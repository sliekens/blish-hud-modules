using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Hero.Equipment.Novelties;

namespace SL.ChatLinks;

public sealed partial class Hero
{
    private IReadOnlyList<Novelty>? _novelties;
    private IReadOnlyList<int>? _unlockedNovelties;

    public async ValueTask<IReadOnlyList<Novelty>> GetNovelties(CancellationToken cancellationToken)
    {
        return _novelties ??= await GetNoveltiesInternal(cancellationToken);
    }

    private async ValueTask<IReadOnlyList<Novelty>> GetNoveltiesInternal(CancellationToken cancellationToken)
    {
        var novelties = await _gw2Client.Hero.Equipment.Novelties
            .GetNovelties(cancellationToken: cancellationToken)
            .ValueOnly();
        return novelties.ToImmutableList();
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedNovelties(CancellationToken cancellationToken)
    {
        return _unlockedNovelties ??= await GetUnlockedNoveltiesInternal(cancellationToken);
    }

    private async ValueTask<IReadOnlyList<int>> GetUnlockedNoveltiesInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            return [];
        }

        var token = await _tokenProvider.GetTokenAsync(cancellationToken);
        var values = await _gw2Client.Hero.Equipment.Novelties
            .GetUnlockedNovelties(token, cancellationToken)
            .ValueOnly();

        return values.ToImmutableList();
    }
}