
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
        await using (sut.ConfigureAwait(true))
        {
            _ = await sut.Database.EnsureDeletedAsync().ConfigureAwait(true);
            await sut.Database.MigrateAsync().ConfigureAwait(true);
            IEnumerable<string> actual = await sut.Database.GetPendingMigrationsAsync().ConfigureAwait(true);

            Assert.Empty(actual);
        }
    }
}
