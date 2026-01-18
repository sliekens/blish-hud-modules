using System.Text.Json;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SL.ChatLinks.Storage.Converters;

public class JsonValueConverter<T>(JsonSerializerOptions? options = null) : ValueConverter<T, string>(
    value => Serialize(value, options),
    value => Deserialize(value, options)
)
{
    private static string Serialize(T value, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(value, options);
    }

    private static T Deserialize(string value, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<T>(value, options)!;
    }
}
