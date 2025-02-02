using GuildWars2;
using GuildWars2.Authorization;

using SL.Common;

namespace SL.ChatLinks;

public sealed partial class Hero : IDisposable
{
    private readonly Gw2Client _gw2Client;

    private readonly ITokenProvider _tokenProvider;

    private readonly IEventAggregator _eventAggregator;

    public Hero(
        Gw2Client gw2Client,
        ITokenProvider tokenProvider,
        IEventAggregator eventAggregator)
    {
        _gw2Client = gw2Client;
        _tokenProvider = tokenProvider;
        _eventAggregator = eventAggregator;
        eventAggregator.Subscribe<AuthorizationInvalidated>(OnAuthorizationInvalidated);
    }

    public bool InventoriesAvailable => _tokenProvider.Grants.Contains(Permission.Inventories);

    public bool UnlocksAvailable => _tokenProvider.Grants.Contains(Permission.Unlocks);

    private async ValueTask OnAuthorizationInvalidated(AuthorizationInvalidated _)
    {
        var unlockedFinishersTask = GetUnlockedFinishersInternal(CancellationToken.None);
        var unlockedGliderSkinsTask = GetUnlockedGliderSkinsInternal(CancellationToken.None);
        var unlockedJadeBotSkinsTask = GetUnlockedJadeBotSkinsInternal(CancellationToken.None);
        var unlockedMailCarriersTask = GetUnlockedMailCarriersInternal(CancellationToken.None);
        var unlockedMistChampionSkinsTask = GetUnlockedMistChampionSkinsInternal(CancellationToken.None);
        var unlockedNoveltiesTask = GetUnlockedNoveltiesInternal(CancellationToken.None);
        var unlockedOutfitsTask = GetUnlockedOutfitsInternal(CancellationToken.None);
        var unlockedWardrobeTask = GetUnlockedWardrobeInternal(CancellationToken.None);
        var unlockedRecipesTask = GetUnlockedRecipesInternal(CancellationToken.None);

        _unlockedFinishers = await unlockedFinishersTask;
        _unlockedGliderSkins = await unlockedGliderSkinsTask;
        _unlockedJadeBotSkins = await unlockedJadeBotSkinsTask;
        _unlockedMailCarriers = await unlockedMailCarriersTask;
        _unlockedMistChampionSkins = await unlockedMistChampionSkinsTask;
        _unlockedNovelties = await unlockedNoveltiesTask;
        _unlockedOutfits = await unlockedOutfitsTask;
        _unlockedWardrobe = await unlockedWardrobeTask;
        _unlockedRecipes = await unlockedRecipesTask;
    }

    public void Dispose()
    {
        _eventAggregator.Unsubscribe<AuthorizationInvalidated>(OnAuthorizationInvalidated);
    }
}