using Blish_HUD.Controls;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

namespace SL.Common.Controls.Items;

public sealed class ItemImage : Image
{
    public ItemImage(Item item, ItemIcons icons)
    {
        Size = new Point(50, 50);
        Texture = icons.GetIcon(item);
    }
}