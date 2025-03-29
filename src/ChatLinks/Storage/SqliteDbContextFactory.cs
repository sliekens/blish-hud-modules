using GuildWars2;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SL.ChatLinks.Storage;

public class SqliteDbContextFactory(IOptions<DatabaseOptions> options, ILoggerFactory loggerFactory) : IDbContextFactory
{
    public ChatLinksContext CreateDbContext(Language language)
    {
        DbContextOptionsBuilder<ChatLinksContext> optionsBuilder = new();
        _ = optionsBuilder.UseLoggerFactory(loggerFactory);
#pragma warning disable CA2000 // Dispose objects before losing scope
        SqliteConnection connection = new(options.Value.ConnectionString(language));
#pragma warning restore CA2000 // Dispose objects before losing scope
        _ = optionsBuilder.UseSqlite(connection);
        Levenshtein.RegisterLevenshteinFunction(connection);
        return new ChatLinksContext(optionsBuilder.Options);
    }

    public ChatLinksContext CreateDbContext(string file)
    {
        DbContextOptionsBuilder<ChatLinksContext> optionsBuilder = new();
        _ = optionsBuilder.UseLoggerFactory(loggerFactory);
#pragma warning disable CA2000 // Dispose objects before losing scope
        SqliteConnection connection = new(options.Value.ConnectionString(file));
#pragma warning restore CA2000 // Dispose objects before losing scope
        _ = optionsBuilder.UseSqlite(connection);
        Levenshtein.RegisterLevenshteinFunction(connection);
        return new ChatLinksContext(optionsBuilder.Options);
    }
}
