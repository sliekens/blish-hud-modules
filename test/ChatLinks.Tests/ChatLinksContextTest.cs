
using Microsoft.EntityFrameworkCore;

using SL.ChatLinks.Storage;

namespace SL.ChatLinks.Tests;

public class ChatLinksContextTest
{
    [Fact]
    public async Task Can_migrate_data()
    {
        DbContextOptions<ChatLinksContext> options = new DbContextOptionsBuilder<ChatLinksContext>()
            .Options;
        ChatLinksContext sut = new(options);

        _ = await sut.Database.EnsureDeletedAsync();
        await sut.Database.MigrateAsync();
        IEnumerable<string> actual = await sut.Database.GetPendingMigrationsAsync();

        Assert.Empty(actual);
    }
}
