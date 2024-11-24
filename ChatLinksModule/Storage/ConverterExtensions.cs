using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatLinksModule.Storage;

internal  static class ConverterExtensions
{
    public static PropertyBuilder<T> HasJsonValueConversion<T>(this PropertyBuilder<T> propertyBuilder)
    {
        return propertyBuilder.HasConversion(new JsonValueConverter<T>());
    }
}