using System.Globalization;

namespace SL.ChatLinks.Storage;

public interface IDbContextFactory
{
    ChatLinksContext CreateDbContext(CultureInfo culture);
}