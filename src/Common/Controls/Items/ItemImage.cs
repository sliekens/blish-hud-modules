using System.Diagnostics;
using System.Net;

using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;

using GuildWars2.Items;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SharpDX.Direct2D1.Effects;

namespace SL.Common.Controls.Items;

public sealed class ItemImage : Image
{
    private static readonly Dictionary<string, AsyncTexture2D> Icons = [];

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
        else if (Icons.TryGetValue(iconUrl, out var found))
        {
            Texture = found;
        }
        else
        {
            var newTexture = new AsyncTexture2D(ContentService.Textures.TransparentPixel);
            Icons[iconUrl] = newTexture;
            Texture = newTexture;
            using var wc = new WebClient();
            wc.DownloadDataTaskAsync(iconUrl).ContinueWith(data =>
            {
                using var ms = new MemoryStream(data.Result);
                newTexture.SwapTexture(TextureUtil.FromStreamPremultiplied(ms));
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}
