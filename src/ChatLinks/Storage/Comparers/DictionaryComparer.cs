using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SL.ChatLinks.Storage.Comparers;

public sealed class DictionaryComparer<TKey, TValue>() : ValueComparer<IDictionary<TKey, TValue>>(
    (left, right) => DictionaryEquals(left, right),
    d => GetDictionaryHashCode(d),
    d => GetSnapshot(d))
{

    private static bool DictionaryEquals(IDictionary<TKey, TValue>? left, IDictionary<TKey, TValue>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left == null || right == null)
        {
            return false;
        }

        if (left.Count != right.Count)
        {
            return false;
        }

        return left.All(pair => right.TryGetValue(pair.Key, out var value) && object.Equals(pair.Value, value));
    }

    private static int GetDictionaryHashCode(IDictionary<TKey, TValue>? dictionary)
    {
        return dictionary?.Aggregate(0, (hash, pair) => HashCode.Combine(hash, pair.Key, pair.Value)) ?? 0;
    }

    private static IDictionary<TKey, TValue> GetSnapshot(IDictionary<TKey, TValue> dictionary) => new Dictionary<TKey, TValue>(dictionary);
}