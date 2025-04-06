using GuildWars2.Authorization;
using GuildWars2.Hero.Achievements;
using GuildWars2.Hero.Banking;
using GuildWars2.Hero.Inventories;

using Microsoft.Extensions.Logging;

using SL.Common.Exploration;

namespace SL.Common.Progression;

public sealed class CurrentAccount : IDisposable
{
    private readonly ILogger<CurrentAccount> _logger;
    private readonly ITokenProvider _tokenProvider;
    private readonly IEventAggregator _eventAggregator;

    private readonly AchievementsProgress _achievementsProgress;
    private readonly AccountBank _bank;
    private readonly AccountMaterialStorage _materialStorage;
    private readonly UnlockedDyes _unlockedDyes;
    private readonly UnlockedFinishers _unlockedFinishers;
    private readonly UnlockedGliderSkins _unlockedGliderSkins;
    private readonly UnlockedJadeBotSkins _unlockedJadeBotSkins;
    private readonly UnlockedMailCarriers _unlockedMailCarriers;
    private readonly UnlockedMiniatures _unlockedMiniatures;
    private readonly UnlockedMistChampionSkins _unlockedMistChampionSkins;
    private readonly UnlockedNovelties _unlockedNovelties;
    private readonly UnlockedOutfits _unlockedOutfits;
    private readonly UnlockedRecipes _unlockedRecipes;
    private readonly UnlockedWardrobe _unlockedWardrobe;

    public CurrentAccount(
        ILogger<CurrentAccount> logger,
        ITokenProvider tokenProvider,
        IEventAggregator eventAggregator,
        AchievementsProgress achievementsProgress,
        AccountBank bank,
        AccountMaterialStorage materialStorage,
        UnlockedDyes unlockedDyes,
        UnlockedFinishers unlockedFinishers,
        UnlockedGliderSkins unlockedGliderSkins,
        UnlockedJadeBotSkins unlockedJadeBotSkins,
        UnlockedMailCarriers unlockedMailCarriers,
        UnlockedMiniatures unlockedMiniatures,
        UnlockedMistChampionSkins unlockedMistChampionSkins,
        UnlockedNovelties unlockedNovelties,
        UnlockedOutfits unlockedOutfits,
        UnlockedRecipes unlockedRecipes,
        UnlockedWardrobe unlockedWardrobe)
    {
        ThrowHelper.ThrowIfNull(eventAggregator);
        _logger = logger;
        _tokenProvider = tokenProvider;
        _eventAggregator = eventAggregator;

        _achievementsProgress = achievementsProgress;
        _bank = bank;
        _materialStorage = materialStorage;
        _unlockedDyes = unlockedDyes;
        _unlockedFinishers = unlockedFinishers;
        _unlockedGliderSkins = unlockedGliderSkins;
        _unlockedJadeBotSkins = unlockedJadeBotSkins;
        _unlockedMailCarriers = unlockedMailCarriers;
        _unlockedMiniatures = unlockedMiniatures;
        _unlockedMistChampionSkins = unlockedMistChampionSkins;
        _unlockedNovelties = unlockedNovelties;
        _unlockedOutfits = unlockedOutfits;
        _unlockedRecipes = unlockedRecipes;
        _unlockedWardrobe = unlockedWardrobe;

        eventAggregator.Subscribe<AuthorizationInvalidated>(OnAuthorizationInvalidated);
        eventAggregator.Subscribe<MapChanged>(OnMapChanged);
    }

    private async Task OnAuthorizationInvalidated(AuthorizationInvalidated _)
    {
        await Validate(true, CancellationToken.None).ConfigureAwait(false);
    }

    private async Task OnMapChanged(MapChanged _)
    {
        await Validate(true, CancellationToken.None).ConfigureAwait(false);
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
        return await _achievementsProgress.GetAchievementProgress(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedDyes(CancellationToken cancellationToken)
    {
        return await _unlockedDyes.GetUnlockedDyes(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedFinishers(CancellationToken cancellationToken)
    {
        return await _unlockedFinishers.GetUnlockedFinishers(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedGliderSkins(CancellationToken cancellationToken)
    {
        return await _unlockedGliderSkins.GetUnlockedGliderSkins(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedJadeBotSkins(CancellationToken cancellationToken)
    {
        return await _unlockedJadeBotSkins.GetUnlockedJadeBotSkins(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedMailCarriers(CancellationToken cancellationToken)
    {
        return await _unlockedMailCarriers.GetUnlockedMailCarriers(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedMiniatures(CancellationToken cancellationToken)
    {
        return await _unlockedMiniatures.GetUnlockedMiniatures(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedMistChampionSkins(CancellationToken cancellationToken)
    {
        return await _unlockedMistChampionSkins.GetUnlockedMistChampionSkins(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedNovelties(CancellationToken cancellationToken)
    {
        return await _unlockedNovelties.GetUnlockedNovelties(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedOutfits(CancellationToken cancellationToken)
    {
        return await _unlockedOutfits.GetUnlockedOutfits(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedRecipes(CancellationToken cancellationToken)
    {
        return await _unlockedRecipes.GetUnlockedRecipes(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedWardrobe(CancellationToken cancellationToken)
    {
        return await _unlockedWardrobe.GetUnlockedWardrobe(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<IReadOnlyList<ItemSlot?>> GetBank(CancellationToken cancellationToken)
    {
        return await _bank.GetBank(cancellationToken).ConfigureAwait(false);
    }


    public async ValueTask<IReadOnlyList<MaterialSlot>> GetMaterialStorage(CancellationToken cancellationToken)
    {
        return await _materialStorage.GetMaterialStorage(cancellationToken).ConfigureAwait(false);
    }

    public async Task Validate(bool force, CancellationToken cancellationToken)
    {
        List<Task> validateTasks =
        [
            _bank.Validate(force, cancellationToken),
            _achievementsProgress.Validate(force, cancellationToken),
            _bank.Validate(force, cancellationToken),
            _materialStorage.Validate(force, cancellationToken),
            _unlockedDyes.Validate(force, cancellationToken),
            _unlockedFinishers.Validate(force, cancellationToken),
            _unlockedGliderSkins.Validate(force, cancellationToken),
            _unlockedJadeBotSkins.Validate(force, cancellationToken),
            _unlockedMailCarriers.Validate(force, cancellationToken),
            _unlockedMiniatures.Validate(force, cancellationToken),
            _unlockedMistChampionSkins.Validate(force, cancellationToken),
            _unlockedNovelties.Validate(force, cancellationToken),
            _unlockedOutfits.Validate(force, cancellationToken),
            _unlockedRecipes.Validate(force, cancellationToken),
            _unlockedWardrobe.Validate(force, cancellationToken)
        ];

        await Task.WhenAll(validateTasks).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _eventAggregator.Unsubscribe<AuthorizationInvalidated>(OnAuthorizationInvalidated);
        _eventAggregator.Unsubscribe<MapChanged>(OnMapChanged);
        _achievementsProgress.Dispose();
        _bank.Dispose();
        _materialStorage.Dispose();
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
