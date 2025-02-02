namespace SL.ChatLinks.Storage.Metadata;

public sealed record Database
{
    public required string Name { get; set; }

    public required long Seed { get; set; }
}