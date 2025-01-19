using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Hero.Equipment.Novelties;

using SL.ChatLinks.Storage;
using SL.Common;

namespace SL.ChatLinks;

public sealed class Hero : IDisposable
{
    private readonly Gw2Client _gw2Client;

    private readonly ITokenProvider _tokenProvider;

    private readonly IEventAggregator _eventAggregator;

    private IReadOnlyList<int>? _unlockedWardrobe;

    private IReadOnlyList<int>? _unlockedNovelties;

    private IReadOnlyList<Novelty>? _novelties;

    public Hero(Gw2Client gw2Client,
        ITokenProvider tokenProvider,
        IEventAggregator eventAggregator)
    {
        _gw2Client = gw2Client;
        _tokenProvider = tokenProvider;
        _eventAggregator = eventAggregator;
        eventAggregator.Subscribe<DatabaseSyncCompleted>(OnDatabaseSyncCompleted);
        eventAggregator.Subscribe<AuthorizationInvalidated>(OnAuthorizationInvalidated);
    }

    public bool UnlocksAvailable => _tokenProvider.Grants.Contains(Permission.Unlocks);

    public async ValueTask<IReadOnlyList<int>> GetUnlockedWardrobe(CancellationToken cancellationToken)
    {
        return _unlockedWardrobe ?? await GetUnlockedWardrobeInternal(cancellationToken);
    }

    private async Task<IReadOnlyList<int>> GetUnlockedWardrobeInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            return [];
        }

        var token = await _tokenProvider.GetTokenAsync(cancellationToken);
        var values = await _gw2Client.Hero.Equipment.Wardrobe
            .GetUnlockedSkins(token, cancellationToken)
            .ValueOnly();

        return values.ToImmutableList();
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedNovelties(CancellationToken cancellationToken)
    {
        return _unlockedNovelties ?? await GetUnlockedNoveltiesInternal(cancellationToken);
    }

    private async Task<IReadOnlyList<int>> GetUnlockedNoveltiesInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            return [];
        }

        var token = await _tokenProvider.GetTokenAsync(cancellationToken);
        var values = await _gw2Client.Hero.Equipment.Novelties
            .GetUnlockedNovelties(token, cancellationToken)
            .ValueOnly();

        return values.ToImmutableList();
    }

    public async ValueTask<IReadOnlyList<Novelty>> GetNovelties(CancellationToken cancellationToken)
    {
        return _novelties ?? await GetNoveltiesInternal(cancellationToken);
    }

    private async Task<IReadOnlyList<Novelty>> GetNoveltiesInternal(CancellationToken cancellationToken)
    {
        var novelties = await _gw2Client.Hero.Equipment.Novelties
            .GetNovelties(cancellationToken: cancellationToken)
            .ValueOnly();
        return novelties.ToImmutableList();
    }

    private async ValueTask OnDatabaseSyncCompleted(DatabaseSyncCompleted _)
    {
        await Refresh();
    }

    private async ValueTask OnAuthorizationInvalidated(AuthorizationInvalidated _)
    {
        await Refresh();
    }

    private async ValueTask Refresh()
    {
        _unlockedWardrobe = await GetUnlockedWardrobeInternal(CancellationToken.None);
        _unlockedNovelties = await GetUnlockedNoveltiesInternal(CancellationToken.None);
        _novelties = await GetNoveltiesInternal(CancellationToken.None);
    }

    public void Dispose()
    {
        _eventAggregator.Unsubscribe<DatabaseSyncCompleted>(OnDatabaseSyncCompleted);
        _eventAggregator.Unsubscribe<AuthorizationInvalidated>(OnAuthorizationInvalidated);
    }
}