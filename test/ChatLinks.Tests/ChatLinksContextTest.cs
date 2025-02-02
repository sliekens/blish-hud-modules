
using Microsoft.EntityFrameworkCore;

using SL.ChatLinks.Storage;

namespace SL.ChatLinks.Tests;

public class ChatLinksContextTest
{
    [Fact]
    public async Task Can_migrate_data()
    {
        var options = new DbContextOptionsBuilder<ChatLinksContext>()
            .Options;
        var sut = new ChatLinksContext(options);

        await sut.Database.EnsureDeletedAsync();
        await sut.Database.MigrateAsync();
        var actual = await sut.Database.GetPendingMigrationsAsync();

        Assert.Empty(actual);
    }
}