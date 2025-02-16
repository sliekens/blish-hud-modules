using System.Text.Json;
using System.Text.Json.Serialization;

namespace SL.ChatLinks.StaticFiles;

public sealed class SeedIndexJsonConverter : JsonConverter<SeedIndex>
{
    public override SeedIndex? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var json = JsonDocument.ParseValue(ref reader);
        var root = json.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        JsonElement databasesElement = default;
        foreach (var property in root.EnumerateObject())
        {
            if (property.NameEquals("databases"))
            {
                databasesElement = property.Value;
            }
        }

        if (databasesElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var databases = new List<SeedDatabase>();
        foreach (var databaseElement in databasesElement.EnumerateArray())
        {
            var database = JsonSerializer.Deserialize<SeedDatabase>(databaseElement.GetRawText(), options);
            if (database is null)
            {
                return null;
            }

            databases.Add(database);
        }

        return new SeedIndex
        {
            Databases = databases
        };
    }

    public override void Write(Utf8JsonWriter writer, SeedIndex value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteStartArray("databases");
        foreach (var database in value.Databases)
        {
            JsonSerializer.Serialize(writer, database, options);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}
