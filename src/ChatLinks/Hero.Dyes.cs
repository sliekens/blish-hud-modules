using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Logging;

namespace SL.ChatLinks;

public sealed partial class Hero
{
    private IReadOnlyList<int>? _unlockedDyes;

    public async ValueTask<IReadOnlyList<int>> GetUnlockedDyes(CancellationToken cancellationToken)
    {
        try
        {
            return _unlockedDyes ??= await GetUnlockedDyesInternal(cancellationToken);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked dyes.");
            return [];
        }
    }

    private async ValueTask<IReadOnlyList<int>> GetUnlockedDyesInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            return [];
        }

        var token = await _tokenProvider.GetTokenAsync(cancellationToken);
        var values = await _gw2Client.Hero.Equipment.Dyes
            .GetUnlockedColors(token, cancellationToken: cancellationToken)
            .ValueOnly();

        return [.. values];
    }
}
