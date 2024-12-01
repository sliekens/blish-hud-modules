using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

namespace ChatLinksModule.UI.Tabs.Items;

public sealed class ItemImage : Image
{
    public ItemImage(Item item)
    {
        Texture = !string.IsNullOrEmpty(item.IconHref)
            ? GameService.Content.GetRenderServiceTexture(item.IconHref)
            : AsyncTexture2D.FromAssetId(1972324);
        Size = new Point(50, 50);
    }
}