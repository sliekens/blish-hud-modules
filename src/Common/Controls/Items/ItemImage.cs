using System.Collections.Concurrent;

using Blish_HUD.Content;
using Blish_HUD.Controls;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

namespace SL.Common.Controls.Items;

public sealed class ItemImage : Image
{
    private static readonly ConcurrentDictionary<string, AsyncTexture2D> WebCache = [];

    public ItemImage(Item item, ItemIcons icons)
    {
        Size = new Point(50, 50);
        Texture = icons.GetIcon(item);
    }
}