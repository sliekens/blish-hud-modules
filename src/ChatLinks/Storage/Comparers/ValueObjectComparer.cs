using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SL.ChatLinks.Storage.Comparers;

public class ValueObjectComparer<T>(Expression<Func<T, T>> snapshotExpression) : ValueComparer<T>(
    (left, right) => object.Equals(left, right),
    obj => obj!.GetHashCode(),
    snapshotExpression);