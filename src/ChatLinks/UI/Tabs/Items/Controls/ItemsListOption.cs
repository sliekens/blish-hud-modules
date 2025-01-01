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
    public ItemIcons Icons { get; }
    public IReadOnlyDictionary<int, UpgradeComponent> Upgrades { get; }

    private readonly ItemImage _icon;

    private readonly ItemName _name;

    public ItemsListOption(Item item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades)
    {
        Item = item;
        Icons = icons;
        Upgrades = upgrades;
        Width = 425;
        Height = 35;
        Menu = new ItemContextMenu(item);

        _icon = new ItemImage(item, icons)
        {
            Size = new Point(35, 35),
            Parent = this
        };

        _name = new ItemName(item, upgrades)
        {
            Left = 40,
            Width = 385,
            AutoSizeHeight = false,
            Height = 35,
            VerticalAlignment = VerticalAlignment.Middle,
            Parent = this
        };
    }

    protected override void OnMouseEntered(MouseEventArgs e)
    {
        BackgroundColor = Color.BurlyWood;
        _name.ShowShadow = true;
        Tooltip tooltip = new(new ItemTooltipView(Item, Icons, Upgrades));
        _icon.Tooltip ??= tooltip;
        _name.Tooltip ??= tooltip;
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
