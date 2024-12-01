using Blish_HUD.Controls;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

namespace ChatLinksModule.UI.Tabs.Items;

public sealed class ItemHeader : FlowPanel
{
    private readonly ItemImage image;

    private readonly ItemName name;

    public ItemHeader(Item item)
    {
        FlowDirection = ControlFlowDirection.SingleLeftToRight;
        ControlPadding = new Vector2(5f, 0);
        image = new ItemImage(item) { Parent = this };
        name = new ItemName(item) { Parent = this };
        Height = 50;
        Width = 290;
    }

    public new Tooltip Tooltip
    {
        get => base.Tooltip;
        set
        {
            base.Tooltip = value;
            image.Tooltip = value;
            name.Tooltip = value;
        }
    }

    public new int Width
    {
        get => base.Width;
        set
        {
            base.Width = value;
            name.Width = value - image.Width;
        }
    }

    public bool Large
    {
        set
        {
            image.Size = value ? new Point(50, 50) : new Point(35, 35);
            name.Height = value ? 50 : 35;
            name.Width = base.Width - image.Width;
        }
    }

    public bool ShowShadow
    {
        get => name.ShowShadow;
        set => name.ShowShadow = value;
    }
}