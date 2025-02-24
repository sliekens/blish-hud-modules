﻿using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Logging;

namespace SL.ChatLinks;

public sealed partial class AccountUnlocks
{
    private IReadOnlyList<int>? _unlockedMiniatures;

    public async ValueTask<IReadOnlyList<int>> GetUnlockedMiniatures(CancellationToken cancellationToken)
    {
        try
        {
            return _unlockedMiniatures ??= await GetUnlockedMiniaturesInternal(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked miniatures.");
            return [];
        }
    }

    private async ValueTask<IReadOnlyList<int>> GetUnlockedMiniaturesInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            return [];
        }

        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        HashSet<int> values = await _gw2Client.Hero.Equipment.Miniatures
            .GetUnlockedMiniatures(token, cancellationToken: cancellationToken)
            .ValueOnly().ConfigureAwait(false);

        return [.. values];
    }
}
