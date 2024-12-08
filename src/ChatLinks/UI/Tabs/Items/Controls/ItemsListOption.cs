using Blish_HUD.Controls;
using Blish_HUD.Input;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.Common.Controls.Items;

using Container = Blish_HUD.Controls.Container;

namespace SL.ChatLinks.UI.Tabs.Items.Controls;

public sealed class ItemsListOption : Container
{
    public Item Item { get; }

    private ItemImage _icon;

    private ItemName _name;

    public ItemsListOption(Item item)
    {
        Item = item;
        Width = 425;
        Height = 35;
        Tooltip = new Tooltip(new ItemTooltipView(Item));

        _icon = new ItemImage(item)
        {
            Size = new Point(35, 35),
            Tooltip = Tooltip,
            Parent = this
        };

        _name = new ItemName(item)
        {
            Text = Item.Name,
            Left = 40,
            Width = 385,
            AutoSizeHeight = false,
            Height = 35,
            VerticalAlignment = VerticalAlignment.Middle,
            Tooltip = Tooltip,
            Parent = this
        };
    }

    protected override void OnMouseEntered(MouseEventArgs e)
    {
        BackgroundColor = Color.BurlyWood;
        _name.ShowShadow = true;
    }

    protected override void OnMouseLeft(MouseEventArgs e)
    {
        BackgroundColor = Color.Transparent;
        _name.ShowShadow = false;
    }

    protected override void DisposeControl()
    {
        _icon.Dispose();
        _name.Dispose();
        base.DisposeControl();
    }
}
