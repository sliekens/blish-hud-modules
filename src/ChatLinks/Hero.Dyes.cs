using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Hero.Equipment.Dyes;

namespace SL.ChatLinks;

public sealed partial class Hero
{
    private IReadOnlyList<DyeColor>? _dyes;

    private IReadOnlyList<int>? _unlockedDyes;

    public async ValueTask<IReadOnlyList<DyeColor>> GetDyes(CancellationToken cancellationToken)
    {
        return _dyes ??= await GetDyesInternal(cancellationToken);
    }

    private async ValueTask<IReadOnlyList<DyeColor>> GetDyesInternal(CancellationToken cancellationToken)
    {
        var dyes = await _gw2Client.Hero.Equipment.Dyes
            .GetColors(cancellationToken: cancellationToken)
            .ValueOnly();
        return dyes.ToImmutableList();
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedDyes(CancellationToken cancellationToken)
    {
        return _unlockedDyes ??= await GetUnlockedDyesInternal(cancellationToken);
    }

    private async ValueTask<IReadOnlyList<int>> GetUnlockedDyesInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            return [];
        }

        var token = await _tokenProvider.GetTokenAsync(cancellationToken);
        var values = await _gw2Client.Hero.Equipment.Dyes
            .GetUnlockedColors(token, cancellationToken: cancellationToken)
            .ValueOnly();

        return values.ToImmutableList();
    }
}