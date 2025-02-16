namespace SL.ChatLinks.Storage.Metadata;

public sealed record Database
{
    public required string Name { get; set; }

    public required int SchemaVersion { get; set; }
}
