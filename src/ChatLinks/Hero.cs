using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;

using SL.ChatLinks.Storage;
using SL.Common;

namespace SL.ChatLinks;

public class Hero
{
    private readonly Gw2Client _gw2Client;

    private readonly ITokenProvider _tokenProvider;

    private IReadOnlyList<int>? _wardrobe;

    public Hero(Gw2Client gw2Client,
        ITokenProvider tokenProvider,
        IEventAggregator eventAggregator)
    {
        _gw2Client = gw2Client;
        _tokenProvider = tokenProvider;
        eventAggregator.Subscribe<DatabaseSyncCompleted>(OnDatabaseSyncCompleted);
        eventAggregator.Subscribe<AuthorizationInvalidated>(OnAuthorizationInvalidated);
    }

    public bool UnlocksAvailable => _tokenProvider.Grants.Contains(Permission.Unlocks);

    public async ValueTask<IReadOnlyList<int>> GetWardrobe(CancellationToken cancellationToken)
    {
        return _wardrobe ?? await GetWardrobeInternal(cancellationToken);
    }

    private async Task<IReadOnlyList<int>> GetWardrobeInternal(CancellationToken cancellationToken)
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

    private async ValueTask OnDatabaseSyncCompleted(DatabaseSyncCompleted obj)
    {
        _wardrobe = await GetWardrobeInternal(CancellationToken.None);
    }

    private async ValueTask OnAuthorizationInvalidated(AuthorizationInvalidated args)
    {
        _wardrobe = await GetWardrobeInternal(CancellationToken.None);
    }
}