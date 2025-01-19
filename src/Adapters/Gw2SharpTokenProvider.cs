using System.Collections.Immutable;

using Blish_HUD;
using Blish_HUD.Modules;

using GuildWars2.Authorization;

using Gw2Sharp.WebApi.V2.Models;

using SL.Common;

namespace SL.Adapters;

public class Gw2SharpTokenProvider : ITokenProvider
{
    private readonly ModuleParameters _parameters;

    private readonly IEventAggregator _eventAggregator;

    public Gw2SharpTokenProvider(ModuleParameters parameters, IEventAggregator eventAggregator)
    {
        _parameters = parameters;
        _eventAggregator = eventAggregator;
        Grants = parameters.Gw2ApiManager.Permissions
            .Select(MapPermission)
            .ToImmutableList();
        parameters.Gw2ApiManager.SubtokenUpdated += OnSubtokenUpdated;
    }

    public string? Token { get; private set; }

    public IReadOnlyList<Permission> Grants { get; private set; }

    private void OnSubtokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> args)
    {
        Token = null;
        Grants = args.Value
            .Select(MapPermission)
            .ToImmutableList();

        _eventAggregator.Publish(new AuthorizationInvalidated(Grants));
    }

    public async Task<string?> GetTokenAsync(CancellationToken cancellationToken)
    {
        return Token ??= await GetTokenInternal(cancellationToken);
    }

    private async Task<string?> GetTokenInternal(CancellationToken cancellationToken)
    {
        var createdSubToken = await _parameters
            .Gw2ApiManager
            .Gw2ApiClient
            .V2
            .CreateSubtoken
            .WithPermissions(_parameters.Gw2ApiManager.Permissions)
            .GetAsync(cancellationToken);

        return createdSubToken.Subtoken;
    }

    private static Permission MapPermission(TokenPermission permission)
    {
        return permission switch
        {
            TokenPermission.Account => Permission.Account,
            TokenPermission.Builds => Permission.Builds,
            TokenPermission.Characters => Permission.Characters,
            TokenPermission.Guilds => Permission.Guilds,
            TokenPermission.Inventories => Permission.Inventories,
            TokenPermission.Progression => Permission.Progression,
            TokenPermission.Pvp => Permission.PvP,
            TokenPermission.Tradingpost => Permission.TradingPost,
            TokenPermission.Unlocks => Permission.Unlocks,
            TokenPermission.Wallet => Permission.Wallet,
            (TokenPermission)11 => Permission.Wvw,
            _ => default
        };
    }
}