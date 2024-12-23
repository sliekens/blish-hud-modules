using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SL.ChatLinks.Storage;

public sealed class ListComparer<T>() : ValueComparer<IReadOnlyList<T>>((left, right) => left.SequenceEqual(right),
    list => list.GetHashCode(),
    list => GetSnapshot(list))
{
    private static IReadOnlyList<T> GetSnapshot(IReadOnlyList<T> list) => [.. list];
}


public sealed class CollectionComparer<T>() : ValueComparer<IReadOnlyCollection<T>>((left, right) => left.SequenceEqual(right),
    collection => collection.GetHashCode(),
    collection => GetSnapshot(collection))
{
    private static IReadOnlyCollection<T> GetSnapshot(IReadOnlyCollection<T> collection) => [.. collection];
}

