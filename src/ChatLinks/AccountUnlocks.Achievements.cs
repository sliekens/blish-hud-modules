using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Hero.Achievements;

using Microsoft.Extensions.Logging;

namespace SL.ChatLinks;

public sealed partial class AccountUnlocks
{
    private IReadOnlyList<AccountAchievement>? _accountAchievements;

    public async ValueTask<IReadOnlyList<AccountAchievement>> GetAccountAchievements(CancellationToken cancellationToken)
    {
        try
        {
            return _accountAchievements ??= await GetAccountAchievementsInternal(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve account achievements.");
            return [];
        }
    }

    private async ValueTask<IReadOnlyList<AccountAchievement>> GetAccountAchievementsInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Progression))
        {
            return [];
        }

        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        HashSet<AccountAchievement> values = await _gw2Client.Hero.Achievements
            .GetAccountAchievements(token, cancellationToken: cancellationToken)
            .ValueOnly()
            .ConfigureAwait(false);

        return [.. values];
    }
}
