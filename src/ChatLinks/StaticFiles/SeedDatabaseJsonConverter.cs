using System.Text.Json;
using System.Text.Json.Serialization;

namespace SL.ChatLinks.StaticFiles;

public sealed class SeedDatabaseJsonConverter : JsonConverter<SeedDatabase>
{
    public override SeedDatabase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        var root = jsonDocument.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        JsonElement versionElement = default;
        JsonElement languageElement = default;
        JsonElement sha256Element = default;
        JsonElement referenceElement = default;

        foreach (var property in root.EnumerateObject())
        {
            if (property.NameEquals("version"))
            {
                versionElement = property.Value;
            }
            else if (property.NameEquals("lang"))
            {
                languageElement = property.Value;
            }
            else if (property.NameEquals("sha256"))
            {
                sha256Element = property.Value;
            }
            else if (property.NameEquals("ref"))
            {
                referenceElement = property.Value;
            }
        }

        if (versionElement.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        int version;
        if (!versionElement.TryGetInt32(out version))
        {
            return null;
        }

        if (languageElement.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        string? language = languageElement.GetString();
        if (languageElement.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(language))
        {
            return null;
        }

        string? sha256 = sha256Element.GetString();
        if (string.IsNullOrWhiteSpace(sha256))
        {
            return null;
        }


        if (referenceElement.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        string? reference = referenceElement.GetString();
        if (string.IsNullOrWhiteSpace(reference))
        {
            return null;
        }

        return new SeedDatabase
        {
            Version = version,
            Language = language!,
            SHA256 = sha256!,
            Reference = reference!
        };
    }

    public override void Write(Utf8JsonWriter writer, SeedDatabase value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("version", value.Version);
        writer.WriteString("lang", value.Language);
        writer.WriteString("sha256", value.SHA256);
        writer.WriteString("ref", value.Reference);
        writer.WriteEndObject();
    }
}