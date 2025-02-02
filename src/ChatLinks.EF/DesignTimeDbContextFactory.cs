using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using SL.ChatLinks.Storage;

using SQLitePCL;

namespace SL.ChatLinks.EF;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ChatLinksContext>
{
    static DesignTimeDbContextFactory()
    {
        Batteries_V2.Init();
    }

    public ChatLinksContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ChatLinksContext>();
        optionsBuilder.UseSqlite("Data Source=X:\\src\\my-blish-modules\\Blish.HUD\\1.2.0\\Settings\\chat-links-data\\data.db", sqliteOptionsBuilder =>
        {
            sqliteOptionsBuilder.MigrationsAssembly("SL.ChatLinks");
        });

        return new ChatLinksContext(optionsBuilder.Options);
    }
}
