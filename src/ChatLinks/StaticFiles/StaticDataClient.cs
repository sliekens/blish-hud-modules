using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
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
		using var response = await httpClient.GetAsync(database.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
		using var content = await response.Content.ReadAsStreamAsync();
		response.EnsureSuccessStatusCode();
		var tmp = Path.GetTempFileName();
		using (var fileStream = File.OpenWrite(tmp))
		{
			await content.CopyToAsync(fileStream, 8192, cancellationToken);
		}

		using (var sha256 = SHA256.Create())
		{
			using var fileStream = File.OpenRead(tmp);
			var hash = sha256.ComputeHash(fileStream);
			var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
			if (hashString != database.SHA256.ToLowerInvariant())
			{
				File.Delete(tmp);
				throw new InvalidOperationException("SHA256 hash mismatch.");
			}
		}

		File.Delete(destination);
		DecompressGzipFile(tmp, destination);
		File.Delete(tmp);
	}

	private void DecompressGzipFile(string sourceFile, string destinationFile)
	{
		using var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
		using var decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress);
		using var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write);
		decompressionStream.CopyTo(destinationStream);
	}
}
