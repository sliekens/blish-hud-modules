using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SL.ChatLinks.Storage.Metadata;

[JsonConverter(typeof(DataManifestJsonConverter))]
public sealed record DataManifest
{
    public required int Version { get; set; }

    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO")]
    public required Dictionary<string, Database> Databases { get; set; }
}
