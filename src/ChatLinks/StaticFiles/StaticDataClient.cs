using System.Net.Http;
using System.Text.Json;

namespace SL.ChatLinks.StaticFiles;

public sealed class StaticDataClient(HttpClient httpClient)
{
    public async Task<SeedIndex> GetSeedIndex(CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync("seed-index.json");
        using var content = await response.Content.ReadAsStreamAsync();
        response.EnsureSuccessStatusCode();
        return await JsonSerializer.DeserializeAsync<SeedIndex>(content, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Couldn't retrieve seed index.");
    }

    public async Task Download(SeedDatabase database, string destination, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(database.Reference);
        using var content = await response.Content.ReadAsStreamAsync();
        response.EnsureSuccessStatusCode();
        using var fileStream = File.Create(destination);
        await content.CopyToAsync(fileStream, 8192, cancellationToken);
    }
}
