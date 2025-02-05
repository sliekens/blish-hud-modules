using GuildWars2.Authorization;

namespace SL.Common;

public interface ITokenProvider
{
    bool IsAuthorized { get; }
    
    IReadOnlyList<Permission> Grants { get; }

    Task<string?> GetTokenAsync(CancellationToken cancellationToken);
}