using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Logging;

namespace SL.ChatLinks;

public sealed partial class AccountUnlocks
{
    private IReadOnlyList<int>? _unlockedNovelties;

    public async ValueTask<IReadOnlyList<int>> GetUnlockedNovelties(CancellationToken cancellationToken)
    {
        try
        {
            return _unlockedNovelties ??= await GetUnlockedNoveltiesInternal(cancellationToken).ConfigureAwait(false);
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

        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        HashSet<int> values = await _gw2Client.Hero.Equipment.Novelties
            .GetUnlockedNovelties(token, cancellationToken)
            .ValueOnly().ConfigureAwait(false);

        return [.. values];
    }
}
