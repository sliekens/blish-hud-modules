using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Logging;

namespace SL.ChatLinks;

public sealed partial class Hero
{
    private IReadOnlyList<int>? _unlockedMailCarriers;


    public async ValueTask<IReadOnlyList<int>> GetUnlockedMailCarriers(CancellationToken cancellationToken)
    {
        try
        {
            return _unlockedMailCarriers ??= await GetUnlockedMailCarriersInternal(cancellationToken);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked mail carriers.");
            return [];
        }
    }

    private async ValueTask<IReadOnlyList<int>> GetUnlockedMailCarriersInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            return [];
        }

        string? token = await _tokenProvider.GetTokenAsync(cancellationToken);
        HashSet<int> values = await _gw2Client.Hero.Equipment.MailCarriers
            .GetUnlockedMailCarriers(token, cancellationToken: cancellationToken)
            .ValueOnly();

        return [.. values];
    }
}
