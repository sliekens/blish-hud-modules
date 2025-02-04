using System.Text.Json.Serialization;

namespace SL.ChatLinks.StaticFiles;

[JsonConverter(typeof(SeedIndexJsonConverter))]
public sealed record SeedIndex
{
    public IReadOnlyList<SeedDatabase> Databases { get; init; } = [];
}

[JsonConverter(typeof(SeedDatabaseJsonConverter))]
public sealed record SeedDatabase
{
    public required int Version { get; init; }

    public required string Language { get; init; }

    public required string SHA256 { get; init; }

    public required string Reference { get; init; }
}
