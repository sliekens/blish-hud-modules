using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using SL.ChatLinks.Storage;

namespace SL.ChatLinks.EF;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ChatLinksContext>
{
    public ChatLinksContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ChatLinksContext>();
        optionsBuilder.UseSqlite("Data Source=data.db", sqliteOptionsBuilder =>
        {
            sqliteOptionsBuilder.MigrationsAssembly("SL.ChatLinks");
        });

        return new ChatLinksContext(optionsBuilder.Options);
    }
}
