using System.Text.Json.Serialization;

namespace SL.ChatLinks.Storage.Metadata;

[JsonConverter(typeof(DataManifestJsonConverter))]
public sealed record DataManifest
{
    public required int Version { get; set; }

    public required Dictionary<string, Database> Databases { get; set; }
}
