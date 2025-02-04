using Microsoft.Extensions.DependencyInjection;

using SL.ChatLinks.StaticFiles;

namespace SL.ChatLinks.Tests;

public class StaticDataClientTest
{
    [Fact]
    public async Task Get_seed_index()
    {
        var services = new ServiceCollection();
        services.AddHttpClient<StaticDataClient>(httpClient =>
            {
                httpClient.BaseAddress = new Uri("https://bhm.blishhud.com/sliekens.chat_links/");
            });

        var sp = services.BuildServiceProvider();

        var sut = sp.GetRequiredService<StaticDataClient>();

        var actual = await sut.GetSeedIndex(CancellationToken.None);

        Assert.NotNull(actual);
        Assert.NotEmpty(actual.Databases);
        Assert.All(actual.Databases, database =>
        {
            Assert.True(database.SchemaVersion >= 3);
            Assert.True(database.Language is "en" or "fr" or "de" or "es");
            Assert.NotEmpty(database.Name);
            Assert.NotEmpty(database.Url);
            Assert.NotEmpty(database.SHA256);
        });
    }
}
