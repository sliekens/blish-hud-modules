using GuildWars2;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace SL.ChatLinks.Storage;

public class SqliteDbContextFactory(IOptions<DatabaseOptions> options) : IDbContextFactory
{
    public ChatLinksContext CreateDbContext(Language language)
    {
        DbContextOptionsBuilder<ChatLinksContext> optionsBuilder = new();
        SqliteConnection connection = new(options.Value.ConnectionString(language));
        _ = optionsBuilder.UseSqlite(connection);
        Levenshtein.RegisterLevenshteinFunction(connection);
        return new ChatLinksContext(optionsBuilder.Options);
    }

    public ChatLinksContext CreateDbContext(string file)
    {
        DbContextOptionsBuilder<ChatLinksContext> optionsBuilder = new();
        SqliteConnection connection = new(options.Value.ConnectionString(file));
        _ = optionsBuilder.UseSqlite(connection);
        Levenshtein.RegisterLevenshteinFunction(connection);
        return new ChatLinksContext(optionsBuilder.Options);
    }
}
