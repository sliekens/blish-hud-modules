using GuildWars2;

namespace SL.ChatLinks.Storage;

public sealed record DatabaseSeeded(Language Language, IReadOnlyDictionary<string, int> Updated);