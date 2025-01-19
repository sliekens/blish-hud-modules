using GuildWars2.Authorization;

namespace SL.Common;

public interface ITokenProvider
{
    public IReadOnlyList<Permission> Grants { get; }

    public Task<string?> GetTokenAsync(CancellationToken cancellationToken);
}