using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SL.ChatLinks.Storage.Converters;

internal static class ConverterExtensions
{
    public static PropertyBuilder<T> HasJsonValueConversion<T>(this PropertyBuilder<T> propertyBuilder)
    {
        return propertyBuilder.HasConversion(new JsonValueConverter<T>());
    }
}
