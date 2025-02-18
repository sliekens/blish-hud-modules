using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SL.ChatLinks.Storage.Comparers;

public sealed class DictionaryComparer<TKey, TValue>() : ValueComparer<IDictionary<TKey, TValue>>(
    (left, right) => DictionaryEquals(left, right),
    d => GetDictionaryHashCode(d),
    d => GetSnapshot(d))
{

    private static bool DictionaryEquals(IDictionary<TKey, TValue>? left, IDictionary<TKey, TValue>? right)
    {
        return ReferenceEquals(left, right)
            || (left != null
                && right != null
                && left.Count == right.Count
                && left.All(pair => right.TryGetValue(pair.Key, out TValue? value)
                    && object.Equals(pair.Value, value)));
    }

    private static int GetDictionaryHashCode(IDictionary<TKey, TValue>? dictionary)
    {
        return dictionary?.Aggregate(0, (hash, pair) => HashCode.Combine(hash, pair.Key, pair.Value)) ?? 0;
    }

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private static IDictionary<TKey, TValue> GetSnapshot(IDictionary<TKey, TValue> dictionary)
    {
        return new Dictionary<TKey, TValue>(dictionary);
    }
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
}
