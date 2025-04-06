using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Hero.Achievements;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using SL.Common.Caching;

namespace SL.Common.Progression;

public sealed class AchievementsProgress(
    ILogger<AchievementsProgress> logger,
    ITokenProvider tokenProvider,
    Gw2Client gw2Client,
    IMemoryCache memoryCache
) : IDisposable
{
    private readonly CacheMasseur<IReadOnlyList<AccountAchievement>> _achievementsProgress = new(memoryCache, "achievements_progress");

    public async ValueTask<IReadOnlyList<AccountAchievement>> GetAchievementProgress(CancellationToken cancellationToken)
    {
        try
        {
            return tokenProvider.Grants.Contains(Permission.Progression)
                ? await _achievementsProgress.GetOrCreate(CacheAchievementProgress, cancellationToken).ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Failed to retrieve account achievements.");
            return [];
        }
    }

    private async ValueTask CacheAchievementProgress(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<AccountAchievement> value, MessageContext context) = await gw2Client.Hero.Achievements
            .GetAccountAchievements(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = value.ToImmutableArray();
    }

    public void ClearCache()
    {
        _achievementsProgress.Clear();
    }

    public void Dispose()
    {
        _achievementsProgress.Dispose();
    }
}
