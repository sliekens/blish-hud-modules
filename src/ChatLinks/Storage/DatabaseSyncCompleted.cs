namespace SL.ChatLinks.Storage;

public record DatabaseSyncCompleted(IReadOnlyDictionary<string, int> Updated);