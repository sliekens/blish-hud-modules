using System.Collections.Concurrent;
using System.Net.Http;

using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;

using GuildWars2.Items;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SL.Common.Controls.Items;

public sealed class ItemImage : Image
{
    private static readonly ConcurrentDictionary<string, AsyncTexture2D> WebCache = [];

    public ItemImage(Item item)
    {
        Size = new Point(50, 50);
        string? iconUrl = item.IconHref;
        if (iconUrl == null)
        {
            return;
        }

        AsyncTexture2D cached = GameService.Content.GetRenderServiceTexture(iconUrl);
        if (cached is not null)
        {
            Texture = cached;
        }
        else
        {
            Texture = WebCache.GetOrAdd(iconUrl, url =>
            {
                HttpClient httpClient = new();
                AsyncTexture2D newTexture = new(ContentService.Textures.TransparentPixel);
                httpClient.GetStreamAsync(url).ContinueWith(task =>
                {
                    try
                    {
                        if (task.Status == TaskStatus.RanToCompletion)
                        {
                            using Stream data = task.Result;
                            Texture2D texture = TextureUtil.FromStreamPremultiplied(data);
                            newTexture.SwapTexture(texture);
                        }
                    }
                    finally
                    {
                        httpClient.Dispose();
                    }
                });

                return newTexture;
            });
        }
    }
}