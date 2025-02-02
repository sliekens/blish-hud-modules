using GuildWars2;

namespace SL.ChatLinks.Storage;

public interface IDbContextFactory
{
    ChatLinksContext CreateDbContext(Language language);
    ChatLinksContext CreateDbContext(string file);
}