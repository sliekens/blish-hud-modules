using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SL.Common;

public sealed class ThrowHelper
{
    [DoesNotReturn]
    public static void ThrowArgumentNull(string? paramName)
    {
        throw new ArgumentNullException(paramName);
    }

    public static void ThrowIfNull(
        [NotNull] object? argument,
        [CallerArgumentExpression(nameof(argument))] string? paramName = null
    )
    {
        if (argument is null)
        {
            ThrowArgumentNull(paramName);
        }
    }
}
