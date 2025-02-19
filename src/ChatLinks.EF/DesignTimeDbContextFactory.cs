using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using SL.ChatLinks.Storage;

using SQLitePCL;

namespace SL.ChatLinks.EF;

internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ChatLinksContext>
{
    static DesignTimeDbContextFactory()
    {
        Batteries_V2.Init();
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
