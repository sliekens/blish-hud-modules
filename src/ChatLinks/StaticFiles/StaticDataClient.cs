using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;

namespace SL.ChatLinks.StaticFiles;

public sealed class StaticDataClient(HttpClient httpClient)
{
    public async Task<SeedIndex> GetSeedIndex(CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.GetAsync("seed-index.json", cancellationToken).ConfigureAwait(false);
        using Stream content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        return await JsonSerializer.DeserializeAsync<SeedIndex>(content, cancellationToken: cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Couldn't retrieve seed index.");
    }

    public async Task Download(SeedDatabase database, string destination, CancellationToken cancellationToken)
    {
        ThrowHelper.ThrowIfNull(database);
        using HttpResponseMessage response = await httpClient.GetAsync(database.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        using Stream content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
        string tmp = Path.GetTempFileName();
        using (FileStream fileStream = File.OpenWrite(tmp))
        {
            await content.CopyToAsync(fileStream, 8192, cancellationToken).ConfigureAwait(false);
        }

        using (SHA256 sha256 = SHA256.Create())
        {
            using FileStream fileStream = File.OpenRead(tmp);
            byte[] hash = sha256.ComputeHash(fileStream);
            string hashString = BitConverter.ToString(hash).Replace("-", "");
            if (!hashString.Equals(database.SHA256, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(tmp);
                throw new InvalidOperationException("SHA256 hash mismatch.");
            }
        }

        File.Delete(destination);
        DecompressGzipFile(tmp, destination);
        File.Delete(tmp);
    }

    private static void DecompressGzipFile(string sourceFile, string destinationFile)
    {
        using FileStream sourceStream = new(sourceFile, FileMode.Open, FileAccess.Read);
        using GZipStream decompressionStream = new(sourceStream, CompressionMode.Decompress);
        using FileStream destinationStream = new(destinationFile, FileMode.Create, FileAccess.Write);
        decompressionStream.CopyTo(destinationStream);
    }
}
