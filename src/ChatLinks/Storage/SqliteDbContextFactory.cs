using System.Globalization;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace SL.ChatLinks.Storage;

public class SqliteDbContextFactory(IOptions<DatabaseOptions> options) : IDbContextFactory
{
    public ChatLinksContext CreateDbContext(CultureInfo culture)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ChatLinksContext>();
        var connection = new SqliteConnection(options.Value.ConnectionString(culture));
        optionsBuilder.UseSqlite(connection);
        Levenshtein.RegisterLevenshteinFunction(connection);
        return new ChatLinksContext(optionsBuilder.Options, culture);
    }
}