using GuildWars2.Collections;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Comparers;

namespace SL.ChatLinks.Storage.Converters;

internal static class ConverterExtensions
{
    public static PropertyBuilder<T> HasJsonValueConversion<T>(this PropertyBuilder<T> propertyBuilder)
    {
        _ = propertyBuilder.HasConversion(new JsonValueConverter<T>());

        ValueComparer<T> comparer = new ValueObjectComparer<T>(obj => obj);
        propertyBuilder.Metadata.SetValueComparer(comparer);
        return propertyBuilder;
    }

    public static PropertyBuilder<IImmutableValueDictionary<TKey, TValue>> HasImmutableValueDictionaryConverter<TKey, TValue>(
        this PropertyBuilder<IImmutableValueDictionary<TKey, TValue>> propertyBuilder,
        Func<string, TKey> keyToProvider,
        Func<TKey, string> keyFromProvider
    )
        where TKey : notnull
    {
        _ = propertyBuilder.HasConversion(new ImmutableValueDictionaryConverter<TKey, TValue>(keyToProvider, keyFromProvider));

        ValueComparer<IImmutableValueDictionary<TKey, TValue>> comparer = new ValueObjectComparer<IImmutableValueDictionary<TKey, TValue>>(dict => dict);
        propertyBuilder.Metadata.SetValueComparer(comparer);
        return propertyBuilder;
    }
}
