using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

namespace SL.ChatLinks;

public sealed partial class Hero
{
    private IReadOnlyList<int>? _unlockedFinishers;

    public async ValueTask<IReadOnlyList<int>> GetUnlockedFinishers(CancellationToken cancellationToken)
    {
        return _unlockedFinishers ??= await GetUnlockedFinishersInternal(cancellationToken);
    }

    private async ValueTask<IReadOnlyList<int>> GetUnlockedFinishersInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            return [];
        }

        var token = await _tokenProvider.GetTokenAsync(cancellationToken);
        var values = await _gw2Client.Hero.Equipment.Finishers
            .GetUnlockedFinishers(token, cancellationToken: cancellationToken)
            .ValueOnly();

        return values.Select(finisher => finisher.Id).ToImmutableList();
    }
}