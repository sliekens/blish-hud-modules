using System.Collections.Immutable;
using System.Text.Json;

using GuildWars2.Collections;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SL.ChatLinks.Storage.Converters;

// BUG: this converter should not be needed  but System.Text.Json has issues with serializing dictionaries with non-string keys
public class ImmutableValueDictionaryConverter<TKey, TValue>(
        Func<string, TKey> keyToProvider,
        Func<TKey, string> keyFromProvider
    ) : ValueConverter<IImmutableValueDictionary<TKey, TValue>, string>(
    value => Serialize(value, keyFromProvider),
    value => Deserialize(value, keyToProvider)
) where TKey : notnull
{
    private static string Serialize(IImmutableValueDictionary<TKey, TValue> value, Func<TKey, string> keyMapper)
    {
        Dictionary<string, TValue>? intermediate = value.ToDictionary(kvp => keyMapper(kvp.Key), kvp => kvp.Value);
        return JsonSerializer.Serialize(intermediate);
    }

    private static ImmutableValueDictionary<TKey, TValue> Deserialize(string value, Func<string, TKey> keyMapper)
    {
        // This is cursed, but can't figure out how to get System.Text.Json to directly deserialize to ImmutableDictionary with key conversion
        Dictionary<string, TValue>? intermediate = JsonSerializer.Deserialize<Dictionary<string, TValue>>(value);
        ImmutableDictionary<TKey, TValue>.Builder builder = ImmutableDictionary.CreateBuilder<TKey, TValue>();
        if (intermediate is not null)
        {
            foreach (KeyValuePair<string, TValue> kvp in intermediate)
            {
                builder.Add(keyMapper(kvp.Key), kvp.Value);
            }
        }
        return new ImmutableValueDictionary<TKey, TValue>(builder.ToImmutable());
    }
}
