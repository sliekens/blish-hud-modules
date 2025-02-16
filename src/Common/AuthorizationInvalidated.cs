using GuildWars2.Authorization;

namespace SL.Common;

public record AuthorizationInvalidated(IReadOnlyList<Permission> Grants);
