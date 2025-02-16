using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Logging;

namespace SL.ChatLinks;

public sealed partial class Hero
{
    private IReadOnlyList<int>? _unlockedNovelties;

    public async ValueTask<IReadOnlyList<int>> GetUnlockedNovelties(CancellationToken cancellationToken)
    {
        try
        {
            return _unlockedNovelties ??= await GetUnlockedNoveltiesInternal(cancellationToken);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked novelties.");
            return [];
        }
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

        return [.. values];
    }
}
