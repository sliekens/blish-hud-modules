using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SL.ChatLinks.Storage.Comparers;

public sealed class ListComparer<T>() : ValueComparer<IReadOnlyList<T>>((left, right) => left.SequenceEqual(right),
    list => list.GetHashCode(),
    list => GetSnapshot(list))
{
    private static IReadOnlyList<T> GetSnapshot(IReadOnlyList<T> list) => [.. list];
}
