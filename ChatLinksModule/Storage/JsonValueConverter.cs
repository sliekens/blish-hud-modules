using System.Text.Json;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ChatLinksModule.Storage;

public class JsonValueConverter<T>() : ValueConverter<T, string>(
    static value => Serialize(value),
    static value => Deserialize(value)
)
{
    private static string Serialize(T value) => JsonSerializer.Serialize(value);

    private static T Deserialize(string value) => JsonSerializer.Deserialize<T>(value)!;
}

