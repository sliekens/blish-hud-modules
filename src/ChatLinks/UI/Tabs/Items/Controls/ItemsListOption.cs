using Blish_HUD.Controls;
using Blish_HUD.Input;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.Common;
using SL.Common.Controls.Items;

using Container = Blish_HUD.Controls.Container;

namespace SL.ChatLinks.UI.Tabs.Items.Controls;

public sealed class ItemsListOption : Container
{
    private readonly Lazy<Tooltip> _tooltip;

    public Item Item { get; }
    public IReadOnlyDictionary<int, UpgradeComponent> Upgrades { get; }

    private readonly ItemImage _icon;

    private readonly ItemName _name;

    public ItemsListOption(Item item, IReadOnlyDictionary<int, UpgradeComponent> upgrades)
    {
        Item = item;
        Upgrades = upgrades;
        Width = 425;
        Height = 35;
        Menu = new ItemContextMenu(item);

        _icon = new ItemImage(item)
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

        _tooltip = new Lazy<Tooltip>(() => new Tooltip(
            ServiceLocator.Resolve<IViewsFactory>().CreateItemTooltipView(item, upgrades))
        );
    }

    protected override void OnMouseEntered(MouseEventArgs e)
    {
        BackgroundColor = Color.BurlyWood;
        _name.ShowShadow = true;
        _icon.Tooltip ??= _tooltip.Value;
        _name.Tooltip ??= _tooltip.Value;
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
