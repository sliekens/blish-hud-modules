using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Logging;

namespace SL.ChatLinks;

public sealed partial class AccountUnlocks
{
    private IReadOnlyList<int>? _unlockedGliderSkins;

    public async ValueTask<IReadOnlyList<int>> GetUnlockedGliderSkins(CancellationToken cancellationToken)
    {
        try
        {
            return _unlockedGliderSkins ??= await GetUnlockedGliderSkinsInternal(cancellationToken);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked gliders.");
            return [];
        }
    }

    private async ValueTask<IReadOnlyList<int>> GetUnlockedGliderSkinsInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            return [];
        }

        string? token = await _tokenProvider.GetTokenAsync(cancellationToken);
        HashSet<int> values = await _gw2Client.Hero.Equipment.Gliders
            .GetUnlockedGliderSkins(token, cancellationToken: cancellationToken)
            .ValueOnly();

        return [.. values];
    }
}
