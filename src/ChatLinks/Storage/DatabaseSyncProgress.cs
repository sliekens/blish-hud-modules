using GuildWars2;

namespace SL.ChatLinks.Storage;

public record DatabaseSyncProgress(string Step, BulkProgress Report);
