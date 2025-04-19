using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using SL.ChatLinks.Storage;

namespace SL.ChatLinks.EF;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812", Justification = "This class is used by Entity Framework tooling.")]
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ChatLinksContext>
{
    static DesignTimeDbContextFactory()
    {
        Sqlite3Setup.Run();
    }

    public ChatLinksContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<ChatLinksContext> optionsBuilder = new();
        _ = optionsBuilder.UseSqlite("Data Source=data.db", sqliteOptionsBuilder =>
        {
            _ = sqliteOptionsBuilder.MigrationsAssembly("SL.ChatLinks");
        });

        return new ChatLinksContext(optionsBuilder.Options);
    }
}
