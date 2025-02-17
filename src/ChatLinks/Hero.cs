using GuildWars2;
using GuildWars2.Authorization;

using Microsoft.Extensions.Logging;

using SL.Common;

namespace SL.ChatLinks;

public sealed partial class Hero : IDisposable
{
    private readonly ILogger<Hero> _logger;

    private readonly Gw2Client _gw2Client;

    private readonly ITokenProvider _tokenProvider;

    private readonly IEventAggregator _eventAggregator;

    public Hero(
        ILogger<Hero> logger,
        Gw2Client gw2Client,
        ITokenProvider tokenProvider,
        IEventAggregator eventAggregator)
    {
        _logger = logger;
        _gw2Client = gw2Client;
        _tokenProvider = tokenProvider;
        _eventAggregator = eventAggregator;
        eventAggregator.Subscribe<AuthorizationInvalidated>(OnAuthorizationInvalidated);
    }

    public bool IsAuthorized => _tokenProvider.IsAuthorized;

    public bool InventoriesAvailable => IsAuthorized && _tokenProvider.Grants.Contains(Permission.Inventories);

    public bool UnlocksAvailable => IsAuthorized && _tokenProvider.Grants.Contains(Permission.Unlocks);

    private async Task OnAuthorizationInvalidated(AuthorizationInvalidated _)
    {
        string? token = await _tokenProvider.GetTokenAsync(CancellationToken.None);
        if (token is null)
        {
            return;
        }

        if (!await HasAccountPermission(token))
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

        try
        {
            _unlockedFinishers = await unlockedFinishersTask;
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked finishers.");
        }

        try
        {
            _unlockedGliderSkins = await unlockedGliderSkinsTask;
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked gliders.");
        }

        try
        {
            _unlockedJadeBotSkins = await unlockedJadeBotSkinsTask;
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked jade bots.");
        }

        try
        {
            _unlockedMailCarriers = await unlockedMailCarriersTask;
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked mail carriers.");
        }

        try
        {
            _unlockedMiniatures = await unlockedMiniaturesTask;
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked miniatures.");
        }

        try
        {
            _unlockedMistChampionSkins = await unlockedMistChampionSkinsTask;
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked mist champions.");
        }

        try
        {
            _unlockedNovelties = await unlockedNoveltiesTask;
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked novelties.");
        }

        try
        {
            _unlockedOutfits = await unlockedOutfitsTask;
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked outfits.");
        }

        try
        {
            _unlockedWardrobe = await unlockedWardrobeTask;
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked skins.");
        }

        try
        {
            _unlockedRecipes = await unlockedRecipesTask;
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
                await Task.Delay(1000);
            }

            try
            {
                TokenInfo tokenInfo = await _gw2Client.Tokens
                    .GetTokenInfo(token, MissingMemberBehavior.Undefined, CancellationToken.None)
                    .ValueOnly();
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
