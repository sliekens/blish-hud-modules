using Blish_HUD;
using Blish_HUD.Modules;

using GuildWars2.Authorization;

using Gw2Sharp.WebApi.V2.Models;

using Microsoft.Extensions.Logging;

using SL.Common;

namespace SL.Adapters;

public class Gw2SharpTokenProvider : ITokenProvider
{
    private readonly ILogger<Gw2SharpTokenProvider> _logger;

    private readonly ModuleParameters _parameters;

    private readonly IEventAggregator _eventAggregator;

    public Gw2SharpTokenProvider(
        ILogger<Gw2SharpTokenProvider> logger,
        ModuleParameters parameters,
        IEventAggregator eventAggregator
    )
    {
        _logger = logger;
        _parameters = parameters;
        _eventAggregator = eventAggregator;
        Grants = [.. parameters.Gw2ApiManager.Permissions
            .Where(parameters.Gw2ApiManager.HasPermission)
            .Select(MapPermission)];
        parameters.Gw2ApiManager.SubtokenUpdated += OnSubtokenUpdated;
    }

    public bool IsAuthorized => _parameters.Gw2ApiManager.HasSubtoken
        && _parameters.Gw2ApiManager.HasPermission(TokenPermission.Account);

    public string? Token { get; private set; }

    public IReadOnlyList<Permission> Grants { get; private set; }

    public async Task<string?> GetTokenAsync(CancellationToken cancellationToken)
    {
        return Token ??= await GetTokenInternal(cancellationToken);
    }

    private async Task<string?> GetTokenInternal(CancellationToken cancellationToken)
    {
        if (!_parameters.Gw2ApiManager.HasSubtoken)
        {
            _logger.LogWarning("API key is missing or subtoken is not yet available.");
            return null;
        }

        try
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
        catch (Exception reason)
        {
            _logger.LogWarning(reason, "Failed to create subtoken.");
            return null;
        }
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

    private void OnSubtokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> args)
    {
        Token = null;
        Grants = [.. args.Value.Select(MapPermission)];

        _eventAggregator.Publish(new AuthorizationInvalidated(Grants));
    }
}
