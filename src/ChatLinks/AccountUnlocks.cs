using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Hero.Achievements;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace SL.ChatLinks;

public sealed partial class AccountUnlocks : IDisposable
{
    private readonly ILogger<AccountUnlocks> _logger;

    private readonly Gw2Client _gw2Client;

    private readonly ITokenProvider _tokenProvider;

    private readonly IEventAggregator _eventAggregator;

    private readonly IMemoryCache _memoryCache;

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
        _memoryCache = memoryCache;
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

    public async ValueTask<IReadOnlyList<AccountAchievement>> GetAchievementProgress(CancellationToken cancellationToken)
    {
        try
        {
            if (_tokenProvider.Grants.Contains(Permission.Progression))
            {
                return await _memoryCache.GetOrCreateAsync(
                    "achievements_progress",
                    async entry =>
                    {
                        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
                        (HashSet<AccountAchievement> value, MessageContext context) = await _gw2Client.Hero.Achievements
                            .GetAccountAchievements(token, cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

                        entry.AbsoluteExpiration = context.Expires;
                        return value.ToImmutableArray();
                    }
                ).ConfigureAwait(false);
            }
            else
            {
                return [];
            }
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve account achievements.");
            return [];
        }
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedDyes(CancellationToken cancellationToken)
    {
        try
        {
            if (_tokenProvider.Grants.Contains(Permission.Unlocks))
            {
                return await _memoryCache.GetOrCreateAsync(
                    "unlocked_dyes",
                    async entry =>
                    {
                        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
                        (HashSet<int> values, MessageContext context) = await _gw2Client.Hero.Equipment.Dyes
                            .GetUnlockedColors(token, cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

                        entry.AbsoluteExpiration = context.Expires;
                        return values.ToImmutableArray();
                    }
                ).ConfigureAwait(false);
            }
            else
            {
                return [];
            }
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked dyes.");
            return [];
        }
    }
    public async ValueTask<IReadOnlyList<int>> GetUnlockedFinishers(CancellationToken cancellationToken)
    {
        try
        {
            if (_tokenProvider.Grants.Contains(Permission.Unlocks))
            {
                return await _memoryCache.GetOrCreateAsync(
                    "unlocked_finishers",
                    async entry =>
                    {
                        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
                        (HashSet<GuildWars2.Hero.Equipment.Finishers.UnlockedFinisher> values, MessageContext context) = await _gw2Client.Hero.Equipment.Finishers
                            .GetUnlockedFinishers(token, cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

                        entry.AbsoluteExpiration = context.Expires;
                        return values.Select(finisher => finisher.Id).ToImmutableArray();
                    }
                ).ConfigureAwait(false);
            }
            else
            {
                return [];
            }
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked finishers.");
            return [];
        }
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedGliderSkins(CancellationToken cancellationToken)
    {
        try
        {
            if (_tokenProvider.Grants.Contains(Permission.Unlocks))
            {
                return await _memoryCache.GetOrCreateAsync(
                    "unlocked_glider_skins",
                    async entry =>
                    {
                        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
                        (HashSet<int> values, MessageContext context) = await _gw2Client.Hero.Equipment.Gliders
                            .GetUnlockedGliderSkins(token, cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

                        entry.AbsoluteExpiration = context.Expires;
                        return values.ToImmutableArray();
                    }
                ).ConfigureAwait(false);
            }
            else
            {
                return [];
            }
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked glider skins.");
            return [];
        }
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedJadeBotSkins(CancellationToken cancellationToken)
    {
        try
        {
            if (_tokenProvider.Grants.Contains(Permission.Unlocks)
                && _tokenProvider.Grants.Contains(Permission.Inventories))
            {
                return await _memoryCache.GetOrCreateAsync(
                    "unlocked_jade_bot_skins",
                    async entry =>
                    {
                        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
                        (HashSet<int> values, MessageContext context) = await _gw2Client.Hero.Equipment.JadeBots
                            .GetUnlockedJadeBotSkins(token, cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

                        entry.AbsoluteExpiration = context.Expires;
                        return values.ToImmutableArray();
                    }
                ).ConfigureAwait(false);
            }
            else
            {
                return [];
            }
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked jade bots.");
            return [];
        }
    }
    public async ValueTask<IReadOnlyList<int>> GetUnlockedMailCarriers(CancellationToken cancellationToken)
    {
        try
        {
            if (_tokenProvider.Grants.Contains(Permission.Unlocks))
            {
                return await _memoryCache.GetOrCreateAsync(
                    "unlocked_mail_carriers",
                    async entry =>
                    {
                        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
                        (HashSet<int> values, MessageContext context) = await _gw2Client.Hero.Equipment.MailCarriers
                            .GetUnlockedMailCarriers(token, cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

                        entry.AbsoluteExpiration = context.Expires;
                        return values.ToImmutableArray();
                    }
                ).ConfigureAwait(false);
            }
            else
            {
                return [];
            }
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked mail carriers.");
            return [];
        }
    }
    public async ValueTask<IReadOnlyList<int>> GetUnlockedMiniatures(CancellationToken cancellationToken)
    {
        try
        {
            if (_tokenProvider.Grants.Contains(Permission.Unlocks))
            {
                return await _memoryCache.GetOrCreateAsync(
                    "unlocked_miniatures",
                    async entry =>
                    {
                        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
                        (HashSet<int> values, MessageContext context) = await _gw2Client.Hero.Equipment.Miniatures
                            .GetUnlockedMiniatures(token, cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

                        entry.AbsoluteExpiration = context.Expires;
                        return values.ToImmutableArray();
                    }
                ).ConfigureAwait(false);
            }
            else
            {
                return [];
            }
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked miniatures.");
            return [];
        }
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedMistChampionSkins(CancellationToken cancellationToken)
    {
        try
        {
            if (_tokenProvider.Grants.Contains(Permission.Unlocks))
            {
                return await _memoryCache.GetOrCreateAsync(
                    "unlocked_mist_champion_skins",
                    async entry =>
                    {
                        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
                        (HashSet<int> values, MessageContext context) = await _gw2Client.Pvp
                            .GetUnlockedMistChampions(token, cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

                        entry.AbsoluteExpiration = context.Expires;
                        return values.ToImmutableArray();
                    }
                ).ConfigureAwait(false);
            }
            else
            {
                return [];
            }
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked mist champions.");
            return [];
        }
    }
    public async ValueTask<IReadOnlyList<int>> GetUnlockedNovelties(CancellationToken cancellationToken)
    {
        try
        {
            if (_tokenProvider.Grants.Contains(Permission.Unlocks))
            {
                return await _memoryCache.GetOrCreateAsync(
                    "unlocked_novelties",
                    async entry =>
                    {
                        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
                        (HashSet<int> values, MessageContext context) = await _gw2Client.Hero.Equipment.Novelties
                            .GetUnlockedNovelties(token, cancellationToken)
                            .ConfigureAwait(false);

                        entry.AbsoluteExpiration = context.Expires;
                        return values.ToImmutableArray();
                    }
                ).ConfigureAwait(false);
            }
            else
            {
                return [];
            }
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked novelties.");
            return [];
        }
    }
    public async ValueTask<IReadOnlyList<int>> GetUnlockedOutfits(CancellationToken cancellationToken)
    {
        try
        {
            if (_tokenProvider.Grants.Contains(Permission.Unlocks))
            {
                return await _memoryCache.GetOrCreateAsync(
                    "unlocked_outfits",
                    async entry =>
                    {
                        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
                        (HashSet<int> values, MessageContext context) = await _gw2Client.Hero.Equipment.Outfits
                            .GetUnlockedOutfits(token, cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

                        entry.AbsoluteExpiration = context.Expires;
                        return values.ToImmutableArray();
                    }
                ).ConfigureAwait(false);
            }
            else
            {
                return [];
            }
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked outfits.");
            return [];
        }
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedRecipes(CancellationToken cancellationToken)
    {
        try
        {
            if (_tokenProvider.Grants.Contains(Permission.Unlocks))
            {
                return await _memoryCache.GetOrCreateAsync(
                    "unlocked_recipes",
                    async entry =>
                    {
                        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
                        (HashSet<int> values, MessageContext context) = await _gw2Client.Hero.Crafting.Recipes
                            .GetUnlockedRecipes(token, cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

                        entry.AbsoluteExpiration = context.Expires;
                        return values.ToImmutableArray();
                    }
                ).ConfigureAwait(false);
            }
            else
            {
                return [];
            }
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked recipes.");
            return [];
        }
    }
    public async ValueTask<IReadOnlyList<int>> GetUnlockedWardrobe(CancellationToken cancellationToken)
    {
        try
        {
            if (_tokenProvider.Grants.Contains(Permission.Unlocks))
            {
                return await _memoryCache.GetOrCreateAsync(
                    "unlocked_wardrobe",
                    async entry =>
                    {
                        string? token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);
                        (HashSet<int> values, MessageContext context) = await _gw2Client.Hero.Equipment.Wardrobe
                            .GetUnlockedSkins(token, cancellationToken)
                            .ConfigureAwait(false);

                        entry.AbsoluteExpiration = context.Expires;
                        return values.ToImmutableArray();
                    }
                ).ConfigureAwait(false);
            }
            else
            {
                return [];
            }
        }
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to retrieve unlocked skins.");
            return [];
        }
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
        _memoryCache.Remove("achievements_progress");
        _memoryCache.Remove("unlocked_dyes");
        _memoryCache.Remove("unlocked_finishers");
        _memoryCache.Remove("unlocked_glider_skins");
        _memoryCache.Remove("unlocked_jade_bot_skins");
        _memoryCache.Remove("unlocked_mail_carriers");
        _memoryCache.Remove("unlocked_miniatures");
        _memoryCache.Remove("unlocked_mist_champion_skins");
        _memoryCache.Remove("unlocked_novelties");
        _memoryCache.Remove("unlocked_outfits");
        _memoryCache.Remove("unlocked_recipes");
        _memoryCache.Remove("unlocked_wardrobe");
    }

    public void Dispose()
    {
        _eventAggregator.Unsubscribe<AuthorizationInvalidated>(OnAuthorizationInvalidated);
        _eventAggregator.Unsubscribe<MapChanged>(OnMapChanged);
        ClearCache();
    }
}
