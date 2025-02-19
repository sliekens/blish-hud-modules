using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Logging;

namespace SL.ChatLinks;

public sealed partial class AccountUnlocks
{
    private IReadOnlyList<int>? _unlockedOutfits;

    public async ValueTask<IReadOnlyList<int>> GetUnlockedOutfits(CancellationToken cancellationToken)
    {
        try
        {
            return _unlockedOutfits ??= await GetUnlockedOutfitsInternal(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked outfits.");
            return [];
        }
    }

    private async ValueTask<IReadOnlyList<int>> GetUnlockedOutfitsInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            return [];
        }

        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        HashSet<int> values = await _gw2Client.Hero.Equipment.Outfits
            .GetUnlockedOutfits(token, cancellationToken: cancellationToken)
            .ValueOnly().ConfigureAwait(false);

        return [.. values];
    }
}
