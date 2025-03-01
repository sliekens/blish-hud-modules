using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Hero.Achievements;

using Microsoft.Extensions.Logging;

namespace SL.ChatLinks;

public sealed partial class AccountUnlocks : IDisposable
{
    private readonly ILogger<AccountUnlocks> _logger;

    private readonly Gw2Client _gw2Client;

    private readonly ITokenProvider _tokenProvider;

    private readonly IEventAggregator _eventAggregator;

    public AccountUnlocks(
        ILogger<AccountUnlocks> logger,
        Gw2Client gw2Client,
        ITokenProvider tokenProvider,
        IEventAggregator eventAggregator)
    {
        ThrowHelper.ThrowIfNull(eventAggregator);
        _logger = logger;
        _gw2Client = gw2Client;
        _tokenProvider = tokenProvider;
        _eventAggregator = eventAggregator;
        eventAggregator.Subscribe<AuthorizationInvalidated>(OnAuthorizationInvalidated);
    }

    public bool IsAuthorized => _tokenProvider.IsAuthorized;

    public bool HasPermission(Permission permission)
    {
        return IsAuthorized && _tokenProvider.Grants.Contains(permission);
    }

    public bool HasPermissions(params Permission[] permissions)
    {
        return IsAuthorized && permissions.All(permission => _tokenProvider.Grants.Contains(permission));
    }

    private async Task OnAuthorizationInvalidated(AuthorizationInvalidated _)
    {
        string? token = await _tokenProvider.GetTokenAsync(CancellationToken.None).ConfigureAwait(false);
        if (token is null)
        {
            return;
        }

        if (!await HasAccountPermission(token).ConfigureAwait(false))
        {
            return;
        }

        ValueTask<IReadOnlyList<int>> unlockedFinishersTask = GetUnlockedFinishersInternal(CancellationToken.None);
        ValueTask<IReadOnlyList<int>> unlockedGliderSkinsTask = GetUnlockedGliderSkinsInternal(CancellationToken.None);
        ValueTask<IReadOnlyList<int>> unlockedJadeBotSkinsTask = GetUnlockedJadeBotSkinsInternal(CancellationToken.None);
        ValueTask<IReadOnlyList<int>> unlockedMailCarriersTask = GetUnlockedMailCarriersInternal(CancellationToken.None);
        ValueTask<IReadOnlyList<int>> unlockedMiniaturesTask = GetUnlockedMiniaturesInternal(CancellationToken.None);
        ValueTask<IReadOnlyList<int>> unlockedMistChampionSkinsTask = GetUnlockedMistChampionSkinsInternal(CancellationToken.None);
        ValueTask<IReadOnlyList<int>> unlockedNoveltiesTask = GetUnlockedNoveltiesInternal(CancellationToken.None);
        ValueTask<IReadOnlyList<int>> unlockedOutfitsTask = GetUnlockedOutfitsInternal(CancellationToken.None);
        ValueTask<IReadOnlyList<int>> unlockedWardrobeTask = GetUnlockedWardrobeInternal(CancellationToken.None);
        ValueTask<IReadOnlyList<int>> unlockedRecipesTask = GetUnlockedRecipesInternal(CancellationToken.None);
        ValueTask<IReadOnlyList<AccountAchievement>> accountAchievementsTask = GetAccountAchievementsInternal(CancellationToken.None);

        try
        {
            _unlockedFinishers = await unlockedFinishersTask.ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked finishers.");
        }

        try
        {
            _unlockedGliderSkins = await unlockedGliderSkinsTask.ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked gliders.");
        }

        try
        {
            _unlockedJadeBotSkins = await unlockedJadeBotSkinsTask.ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked jade bots.");
        }

        try
        {
            _unlockedMailCarriers = await unlockedMailCarriersTask.ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked mail carriers.");
        }

        try
        {
            _unlockedMiniatures = await unlockedMiniaturesTask.ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked miniatures.");
        }

        try
        {
            _unlockedMistChampionSkins = await unlockedMistChampionSkinsTask.ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked mist champions.");
        }

        try
        {
            _unlockedNovelties = await unlockedNoveltiesTask.ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked novelties.");
        }

        try
        {
            _unlockedOutfits = await unlockedOutfitsTask.ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked outfits.");
        }

        try
        {
            _unlockedWardrobe = await unlockedWardrobeTask.ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked skins.");
        }

        try
        {
            _unlockedRecipes = await unlockedRecipesTask.ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked recipes.");
        }

        try
        {
            _accountAchievements = await accountAchievementsTask.ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked recipes.");
        }
    }

    private async Task<bool> HasAccountPermission(string token)
    {
        // Subtokens are not immediately authorized after creation, do a few retries.
        int attempt = 0;
        while (attempt < 10)
        {
            if (attempt > 0)
            {
                await Task.Delay(1000).ConfigureAwait(false);
            }

            try
            {
                TokenInfo tokenInfo = await _gw2Client.Tokens
                    .GetTokenInfo(token, MissingMemberBehavior.Undefined, CancellationToken.None)
                    .ValueOnly().ConfigureAwait(false);
                return tokenInfo.Permissions.Contains(Permission.Account);
            }
            catch (Exception reason)
            {
                _logger.LogWarning(reason, "Failed to refresh token info.");
                attempt++;
            }
        }

        return false;
    }

    public void Dispose()
    {
        _eventAggregator.Unsubscribe<AuthorizationInvalidated>(OnAuthorizationInvalidated);
    }
}
