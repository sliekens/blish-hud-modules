using System.Text.Json;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SL.ChatLinks.Storage.Converters;

public class JsonValueConverter<T>() : ValueConverter<T, string>(
    static value => Serialize(value),
    static value => Deserialize(value)
)
{
    private static string Serialize(T value)
    {
        return JsonSerializer.Serialize(value);
    }

    private static T Deserialize(string value)
    {
        return JsonSerializer.Deserialize<T>(value)!;
    }
}
