using Blish_HUD.Controls;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using Container = Blish_HUD.Controls.Container;

namespace ChatLinksModule.UI.Tabs.Items.Controls;

public sealed class ItemHeader : Container
{
    private readonly ItemName _name;

    public ItemHeader(Item item)
    {
        ItemImage image = new(item)
        {
            Parent = this,
            Size = new Point(50)
        };

        _name = new ItemName(item)
        {
            Parent = this,
            Left = 55,
            Width = 235
        };

        Height = 50;
        Width = 290;
        Tooltip = new Tooltip(new ItemTooltipView(item));
        image.Tooltip = Tooltip;
        _name.Tooltip = Tooltip;
    }

    public int Quantity
    {
        get => _name.Quantity;
        set => _name.Quantity = value;
    }
}