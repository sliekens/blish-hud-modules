﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace SL.ChatLinks.StaticFiles;

public sealed class SeedIndexJsonConverter : JsonConverter<SeedIndex>
{
    public override SeedIndex? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument json = JsonDocument.ParseValue(ref reader);
        JsonElement root = json.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        JsonElement databasesElement = default;
        foreach (JsonProperty property in root.EnumerateObject())
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

        List<SeedDatabase> databases = [];
        foreach (JsonElement databaseElement in databasesElement.EnumerateArray())
        {
            SeedDatabase? database = JsonSerializer.Deserialize<SeedDatabase>(databaseElement.GetRawText(), options);
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
        ThrowHelper.ThrowIfNull(writer);
        ThrowHelper.ThrowIfNull(value);
        writer.WriteStartObject();
        writer.WriteStartArray("databases");
        foreach (SeedDatabase database in value.Databases)
        {
            JsonSerializer.Serialize(writer, database, options);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}
