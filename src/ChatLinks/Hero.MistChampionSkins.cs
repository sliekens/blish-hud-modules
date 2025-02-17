using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Logging;

namespace SL.ChatLinks;

public sealed partial class Hero
{
    private IReadOnlyList<int>? _unlockedMistChampionSkins;

    public async ValueTask<IReadOnlyList<int>> GetUnlockedMistChampionSkins(CancellationToken cancellationToken)
    {
        try
        {
            return _unlockedMistChampionSkins ??= await GetUnlockedMistChampionSkinsInternal(cancellationToken);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked mist champions.");
            return [];
        }
    }

    private async ValueTask<IReadOnlyList<int>> GetUnlockedMistChampionSkinsInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            return [];
        }

        string? token = await _tokenProvider.GetTokenAsync(cancellationToken);
        HashSet<int> values = await _gw2Client.Pvp
            .GetUnlockedMistChampions(token, cancellationToken: cancellationToken)
            .ValueOnly();

        return [.. values];
    }
}
