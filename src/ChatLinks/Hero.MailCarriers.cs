using System.Collections.Immutable;

using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Hero.Equipment.MailCarriers;

namespace SL.ChatLinks;

public sealed partial class Hero
{
    private IReadOnlyList<MailCarrier>? _mailCarriers;

    private IReadOnlyList<int>? _unlockedMailCarriers;

    public async ValueTask<IReadOnlyList<MailCarrier>> GetMailCarriers(CancellationToken cancellationToken)
    {
        return _mailCarriers ??= await GetMailCarriersInternal(cancellationToken);
    }

    private async ValueTask<IReadOnlyList<MailCarrier>> GetMailCarriersInternal(CancellationToken cancellationToken)
    {
        var mailCarriers = await _gw2Client.Hero.Equipment.MailCarriers
            .GetMailCarriers(cancellationToken: cancellationToken)
            .ValueOnly();
        return mailCarriers.ToImmutableList();
    }

    public async ValueTask<IReadOnlyList<int>> GetUnlockedMailCarriers(CancellationToken cancellationToken)
    {
        return _unlockedMailCarriers ??= await GetUnlockedMailCarriersInternal(cancellationToken);
    }

    private async ValueTask<IReadOnlyList<int>> GetUnlockedMailCarriersInternal(CancellationToken cancellationToken)
    {
        if (!_tokenProvider.Grants.Contains(Permission.Unlocks))
        {
            return [];
        }

        var token = await _tokenProvider.GetTokenAsync(cancellationToken);
        var values = await _gw2Client.Hero.Equipment.MailCarriers
            .GetUnlockedMailCarriers(token, cancellationToken: cancellationToken)
            .ValueOnly();

        return values.ToImmutableList();
    }
}