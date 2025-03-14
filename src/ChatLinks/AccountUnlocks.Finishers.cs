﻿using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Logging;

namespace SL.ChatLinks;

public sealed partial class AccountUnlocks
{
    private IReadOnlyList<int>? _unlockedFinishers;

    public async ValueTask<IReadOnlyList<int>> GetUnlockedFinishers(CancellationToken cancellationToken)
    {
        try
        {
            return _unlockedFinishers ??= await GetUnlockedFinishersInternal(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked finishers.");
            return [];
        }
    }

    private async ValueTask<IReadOnlyList<int>> GetUnlockedFinishersInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            return [];
        }

        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        HashSet<GuildWars2.Hero.Equipment.Finishers.UnlockedFinisher> values = await _gw2Client.Hero.Equipment.Finishers
            .GetUnlockedFinishers(token, cancellationToken: cancellationToken)
            .ValueOnly().ConfigureAwait(false);

        return [.. values.Select(finisher => finisher.Id)];
    }
}
