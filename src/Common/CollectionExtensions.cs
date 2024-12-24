using System.Collections.ObjectModel;

namespace SL.Common;

public static class CollectionExtensions
{
    public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary)
    {
        if (dictionary is null)
        {
            throw new ArgumentNullException(nameof(dictionary));
        }

        return dictionary as ReadOnlyDictionary<TKey, TValue>
               ?? new ReadOnlyDictionary<TKey, TValue>(dictionary);
    }
}