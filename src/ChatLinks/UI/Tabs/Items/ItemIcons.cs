﻿using System.Collections.Concurrent;
using System.Net.Http;

using Blish_HUD;
using Blish_HUD.Content;

using GuildWars2.Items;

using Microsoft.Xna.Framework.Graphics;

namespace SL.ChatLinks.UI.Tabs.Items;

public class ItemIcons(HttpClient httpClient)
{
    private static readonly ConcurrentDictionary<string, AsyncTexture2D> WebCache = [];

    public AsyncTexture2D? GetIcon(Item item)
    {
        ThrowHelper.ThrowIfNull(item);
        if (item.IconHref is null)
        {
            return null;
        }

        AsyncTexture2D? cached = GameService.Content.GetRenderServiceTexture(item.IconHref)
            ?? WebCache.GetOrAdd(item.IconHref, url =>
            {
                AsyncTexture2D newTexture = new();
                _ = httpClient.GetStreamAsync(new Uri(url)).ContinueWith(task =>
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
