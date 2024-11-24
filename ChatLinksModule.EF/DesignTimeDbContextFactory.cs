using System.Collections;

using ChatLinksModule.Storage;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChatLinksModule.Design;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ChatLinksContext>
{
    public ChatLinksContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ChatLinksContext>();
        optionsBuilder.UseSqlite("Data Source=data.db", sqliteOptionsBuilder =>
        {
            sqliteOptionsBuilder.MigrationsAssembly("ChatLinksModule");
        });

        return new ChatLinksContext(optionsBuilder.Options);
    }
}