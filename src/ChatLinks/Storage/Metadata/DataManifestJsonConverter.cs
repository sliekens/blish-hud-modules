using System.Text.Json;
using System.Text.Json.Serialization;

namespace SL.ChatLinks.Storage.Metadata;

public sealed class DataManifestJsonConverter : JsonConverter<DataManifest>
{
    public override DataManifest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument json = JsonDocument.ParseValue(ref reader);
        if (json.RootElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!json.RootElement.TryGetProperty("version", out JsonElement versionElement))
        {
            return null;
        }

        if (versionElement.ValueKind != JsonValueKind.Number)
        {
            return null;
        }

        int version = versionElement.GetInt32();
        if (version != 1)
        {
            return null;
        }

        if (!json.RootElement.TryGetProperty("databases", out JsonElement databaseVersionsElement))
        {
            return null;
        }

        if (databaseVersionsElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }


        Dictionary<string, Database> databases = [];
        foreach (JsonProperty database in databaseVersionsElement.EnumerateObject())
        {
            if (database.Value.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            if (!database.Value.TryGetProperty("name", out JsonElement nameElement))
            {
                continue;
            }

            if (nameElement.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            if (!database.Value.TryGetProperty("schema_version", out JsonElement schemaVersionElement))
            {
                continue;
            }

            if (schemaVersionElement.ValueKind != JsonValueKind.Number)
            {
                continue;
            }

            if (!schemaVersionElement.TryGetInt32(out int schemaVersion))
            {
                continue;
            }

            string? name = nameElement.GetString();
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            databases.Add(database.Name, new Database
            {
                Name = name!,
                SchemaVersion = schemaVersion
            });
        }

        return new DataManifest
        {
            Version = version,
            Databases = databases
        };
    }

    public override void Write(Utf8JsonWriter writer, DataManifest value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("version", value.Version);
        writer.WriteStartObject("databases");
        foreach (KeyValuePair<string, Database> pair in value.Databases)
        {
            writer.WriteStartObject(pair.Key);
            writer.WriteString("name", pair.Value.Name);
            writer.WriteNumber("schema_version", pair.Value.SchemaVersion);
            writer.WriteEndObject();
        }
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
