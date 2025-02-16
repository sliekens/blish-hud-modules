using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Logging;

namespace SL.ChatLinks;

public sealed partial class Hero
{
    private IReadOnlyList<int>? _unlockedJadeBotSkins;

    public async ValueTask<IReadOnlyList<int>> GetUnlockedJadeBotSkins(CancellationToken cancellationToken)
    {
        try
        {
            return _unlockedJadeBotSkins ??= await GetUnlockedJadeBotSkinsInternal(cancellationToken);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked jade bots.");
            return [];
        }
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

        return [.. values];
    }
}
