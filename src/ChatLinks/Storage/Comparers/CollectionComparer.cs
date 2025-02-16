using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SL.ChatLinks.Storage.Comparers;

public sealed class CollectionComparer<T>() : ValueComparer<IReadOnlyCollection<T>>((left, right) => left.SequenceEqual(right),
    collection => collection.GetHashCode(),
    collection => GetSnapshot(collection))
{
    private static IReadOnlyCollection<T> GetSnapshot(IReadOnlyCollection<T> collection) => [.. collection];
}
