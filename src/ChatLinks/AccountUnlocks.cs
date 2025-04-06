using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Hero.Achievements;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace SL.ChatLinks;

public sealed class AccountUnlocks : IDisposable
{
    private readonly ILogger<AccountUnlocks> _logger;

    private readonly Gw2Client _gw2Client;

    private readonly ITokenProvider _tokenProvider;

    private readonly IEventAggregator _eventAggregator;

    private readonly CacheMasseur<IReadOnlyList<AccountAchievement>> _achievementsProgress;

    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedDyes;

    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedFinishers;

    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedGliderSkins;

    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedJadeBotSkins;

    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedMailCarriers;

    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedMiniatures;

    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedMistChampionSkins;

    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedNovelties;

    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedOutfits;

    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedRecipes;

    private readonly CacheMasseur<IReadOnlyList<int>> _unlockedWardrobe;

    public AccountUnlocks(
        ILogger<AccountUnlocks> logger,
        Gw2Client gw2Client,
        ITokenProvider tokenProvider,
        IEventAggregator eventAggregator,
        IMemoryCache memoryCache)
    {
        ThrowHelper.ThrowIfNull(eventAggregator);
        _logger = logger;
        _gw2Client = gw2Client;
        _tokenProvider = tokenProvider;
        _eventAggregator = eventAggregator;
        _achievementsProgress =
            new CacheMasseur<IReadOnlyList<AccountAchievement>>(memoryCache, "achievements_progress");
        _unlockedDyes = new CacheMasseur<IReadOnlyList<int>>(memoryCache, "unlocked_dyes");
        _unlockedFinishers = new CacheMasseur<IReadOnlyList<int>>(memoryCache, "unlocked_finishers");
        _unlockedGliderSkins = new CacheMasseur<IReadOnlyList<int>>(memoryCache, "unlocked_glider_skins");
        _unlockedJadeBotSkins = new CacheMasseur<IReadOnlyList<int>>(memoryCache, "unlocked_jade_bot_skins");
        _unlockedMailCarriers = new CacheMasseur<IReadOnlyList<int>>(memoryCache, "unlocked_mail_carriers");
        _unlockedMiniatures = new CacheMasseur<IReadOnlyList<int>>(memoryCache, "unlocked_miniatures");
        _unlockedMistChampionSkins = new CacheMasseur<IReadOnlyList<int>>(memoryCache, "unlocked_mist_champion_skins");
        _unlockedNovelties = new CacheMasseur<IReadOnlyList<int>>(memoryCache, "unlocked_novelties");
        _unlockedOutfits = new CacheMasseur<IReadOnlyList<int>>(memoryCache, "unlocked_outfits");
        _unlockedRecipes = new CacheMasseur<IReadOnlyList<int>>(memoryCache, "unlocked_recipes");
        _unlockedWardrobe = new CacheMasseur<IReadOnlyList<int>>(memoryCache, "unlocked_wardrobe");
        eventAggregator.Subscribe<AuthorizationInvalidated>(OnAuthorizationInvalidated);
        eventAggregator.Subscribe<MapChanged>(OnMapChanged);
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

    public async ValueTask<IReadOnlyList<AccountAchievement>> GetAchievementProgress(
        CancellationToken cancellationToken)
    {
        try
        {
            return _tokenProvider.Grants.Contains(Permission.Progression)
                ? await _achievementsProgress.GetOrCreate(CacheAchievementProgress, cancellationToken)
                    .ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve account achievements.");
            return [];
        }
    }

    private async ValueTask CacheAchievementProgress(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<AccountAchievement> value, MessageContext context) = await _gw2Client.Hero.Achievements
            .GetAccountAchievements(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = value.ToImmutableArray();
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedDyes(CancellationToken cancellationToken)
    {
        try
        {
            return _tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedDyes.GetOrCreate(CacheUnlockedDyes, cancellationToken)
                    .ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked dyes.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedDyes(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<int> values, MessageContext context) = await _gw2Client.Hero.Equipment.Dyes
            .GetUnlockedColors(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedFinishers(CancellationToken cancellationToken)
    {
        try
        {
            return _tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedFinishers
                    .GetOrCreate(CacheUnlockedFinishers, cancellationToken)
                    .ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked finishers.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedFinishers(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<GuildWars2.Hero.Equipment.Finishers.UnlockedFinisher> values, MessageContext context) =
            await _gw2Client.Hero.Equipment.Finishers
                .GetUnlockedFinishers(token, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.Select(finisher => finisher.Id).ToImmutableArray();
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedGliderSkins(CancellationToken cancellationToken)
    {
        try
        {
            return _tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedGliderSkins.GetOrCreate(CacheUnlockedGliderSkins, cancellationToken)
                    .ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked glider skins.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedGliderSkins(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<int> values, MessageContext context) = await _gw2Client.Hero.Equipment.Gliders
            .GetUnlockedGliderSkins(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedJadeBotSkins(CancellationToken cancellationToken)
    {
        try
        {
            return _tokenProvider.Grants.Contains(Permission.Unlocks)
                   && _tokenProvider.Grants.Contains(Permission.Inventories)
                ? await _unlockedJadeBotSkins.GetOrCreate(CacheUnlockedJadeBotSkins, cancellationToken)
                    .ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked jade bots.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedJadeBotSkins(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<int> values, MessageContext context) = await _gw2Client.Hero.Equipment.JadeBots
            .GetUnlockedJadeBotSkins(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedMailCarriers(CancellationToken cancellationToken)
    {
        try
        {
            return _tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedMailCarriers.GetOrCreate(CacheUnlockedMailCarriers, cancellationToken)
                    .ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked mail carriers.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedMailCarriers(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<int> values, MessageContext context) = await _gw2Client.Hero.Equipment.MailCarriers
            .GetUnlockedMailCarriers(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedMiniatures(CancellationToken cancellationToken)
    {
        try
        {
            return _tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedMiniatures.GetOrCreate(CacheUnlockedMiniatures, cancellationToken)
                    .ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked miniatures.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedMiniatures(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<int> values, MessageContext context) = await _gw2Client.Hero.Equipment.Miniatures
            .GetUnlockedMiniatures(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedMistChampionSkins(CancellationToken cancellationToken)
    {
        try
        {
            return _tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedMistChampionSkins.GetOrCreate(CacheUnlockedMistChampionSkins, cancellationToken)
                    .ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked mist champions.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedMistChampionSkins(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<int> values, MessageContext context) = await _gw2Client.Pvp
            .GetUnlockedMistChampions(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedNovelties(CancellationToken cancellationToken)
    {
        try
        {
            return _tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedNovelties.GetOrCreate(CacheUnlockedNovelties, cancellationToken).ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked novelties.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedNovelties(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<int> values, MessageContext context) = await _gw2Client.Hero.Equipment.Novelties
            .GetUnlockedNovelties(token, cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedOutfits(CancellationToken cancellationToken)
    {
        try
        {
            return _tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedOutfits.GetOrCreate(CacheUnlockedOutfits, cancellationToken).ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked outfits.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedOutfits(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<int> values, MessageContext context) = await _gw2Client.Hero.Equipment.Outfits
            .GetUnlockedOutfits(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedRecipes(CancellationToken cancellationToken)
    {
        try
        {
            return _tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedRecipes.GetOrCreate(CacheUnlockedRecipes, cancellationToken).ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked recipes.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedRecipes(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<int> values, MessageContext context) = await _gw2Client.Hero.Crafting.Recipes
            .GetUnlockedRecipes(token, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedWardrobe(CancellationToken cancellationToken)
    {
        try
        {
            return _tokenProvider.Grants.Contains(Permission.Unlocks)
                ? await _unlockedWardrobe.GetOrCreate(CacheUnlockedWardrobe, cancellationToken).ConfigureAwait(false)
                : [];
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked skins.");
            return [];
        }
    }

    private async ValueTask CacheUnlockedWardrobe(ICacheEntry entry, CancellationToken cancellationToken)
    {
        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        (HashSet<int> values, MessageContext context) = await _gw2Client.Hero.Equipment.Wardrobe
            .GetUnlockedSkins(token, cancellationToken)
            .ConfigureAwait(false);

        entry.AbsoluteExpiration = context.Expires;
        entry.Value = values.ToImmutableArray();
    }

    private void OnAuthorizationInvalidated(AuthorizationInvalidated _)
    {
        ClearCache();
    }

    private void OnMapChanged(MapChanged _)
    {
        ClearCache();
    }

    private void ClearCache()
    {
        _achievementsProgress.Clear();
        _unlockedDyes.Clear();
        _unlockedFinishers.Clear();
        _unlockedGliderSkins.Clear();
        _unlockedJadeBotSkins.Clear();
        _unlockedMailCarriers.Clear();
        _unlockedMiniatures.Clear();
        _unlockedMistChampionSkins.Clear();
        _unlockedNovelties.Clear();
        _unlockedOutfits.Clear();
        _unlockedRecipes.Clear();
        _unlockedWardrobe.Clear();
    }

    public void Dispose()
    {
        _eventAggregator.Unsubscribe<AuthorizationInvalidated>(OnAuthorizationInvalidated);
        _eventAggregator.Unsubscribe<MapChanged>(OnMapChanged);
        _achievementsProgress.Dispose();
        _unlockedDyes.Dispose();
        _unlockedFinishers.Dispose();
        _unlockedGliderSkins.Dispose();
        _unlockedJadeBotSkins.Dispose();
        _unlockedMailCarriers.Dispose();
        _unlockedMiniatures.Dispose();
        _unlockedMistChampionSkins.Dispose();
        _unlockedNovelties.Dispose();
        _unlockedOutfits.Dispose();
        _unlockedRecipes.Dispose();
        _unlockedWardrobe.Dispose();
    }
}
