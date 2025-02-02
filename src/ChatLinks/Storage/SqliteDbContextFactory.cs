using GuildWars2;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace SL.ChatLinks.Storage;

public class SqliteDbContextFactory(IOptions<DatabaseOptions> options) : IDbContextFactory
{
    public ChatLinksContext CreateDbContext(Language language)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ChatLinksContext>();
        var connection = new SqliteConnection(options.Value.ConnectionString(language));
        optionsBuilder.UseSqlite(connection);
        Levenshtein.RegisterLevenshteinFunction(connection);
        return new ChatLinksContext(optionsBuilder.Options);
    }

    public ChatLinksContext CreateDbContext(string file)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ChatLinksContext>();
        var connection = new SqliteConnection(options.Value.ConnectionString(file));
        optionsBuilder.UseSqlite(connection);
        Levenshtein.RegisterLevenshteinFunction(connection);
        return new ChatLinksContext(optionsBuilder.Options);
    }
}