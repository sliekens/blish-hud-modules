using System.Net.Http;

using Blish_HUD;
using Blish_HUD.Content;

using Microsoft.Xna.Framework.Graphics;

namespace SL.ChatLinks.UI;

public sealed class IconsService(HttpClient httpClient, IconsCache cache)
{
    public AsyncTexture2D? GetIcon(Uri? iconUrl)
    {
        if (iconUrl is null)
        {
            return null;
        }

        AsyncTexture2D cached = GameService.Content.GetRenderServiceTexture(iconUrl)
            ?? cache.GetOrAdd(iconUrl, url =>
            {
                AsyncTexture2D newTexture = new();
                _ = httpClient.GetStreamAsync(url).ContinueWith(task =>
                {
                    if (task.Status != TaskStatus.RanToCompletion)
                    {
                        return;
                    }

                    using Stream data = task.Result;
                    Texture2D texture = TextureUtil.FromStreamPremultiplied(data);
                    newTexture.SwapTexture(texture);
                }, TaskScheduler.Default);

                return newTexture;
            });

        return cached.Duplicate();
    }
}
