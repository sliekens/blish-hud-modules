using Blish_HUD.Controls;
using Blish_HUD.Input;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using Container = Blish_HUD.Controls.Container;

namespace ChatLinksModule.UI.Tabs.Items.Controls;

public class ItemChoice : Container
{
    private readonly ItemHeader _itemHeader;

    public ItemChoice(Item item)
    {
        _itemHeader = new ItemHeader(item)
        {
            Parent = this,
            Tooltip = new Tooltip(new ItemTooltipView(item)),
            Large = false,
            Width = 430
        };

        Size = new Point(430, 35);
        Item = item;
    }

    public Item Item { get; }

    protected override void OnMouseEntered(MouseEventArgs e)
    {
        BackgroundColor = Color.BurlyWood;
        _itemHeader.ShowShadow = true;
    }

    protected override void OnMouseLeft(MouseEventArgs e)
    {
        BackgroundColor = Color.Transparent;
        _itemHeader.ShowShadow = false;
    }

    protected override void DisposeControl()
    {
        _itemHeader.Dispose();
        base.DisposeControl();
    }
}