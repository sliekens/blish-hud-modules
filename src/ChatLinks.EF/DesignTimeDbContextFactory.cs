using System.Globalization;

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
        optionsBuilder.UseSqlite("Data Source=data.db", sqliteOptionsBuilder =>
        {
            sqliteOptionsBuilder.MigrationsAssembly("SL.ChatLinks");
        });

        return new ChatLinksContext(optionsBuilder.Options, CultureInfo.InvariantCulture);
    }
}
