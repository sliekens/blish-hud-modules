using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using Container = Blish_HUD.Controls.Container;

namespace ChatLinksModule.UI.Tabs.Items.Controls;

public sealed class ItemsListOption : Container
{
    public Item Item { get; }

    private Image _icon;

    private Label _name;

    public ItemsListOption(Item item)
    {
        Item = item;
        Width = 425;
        Height = 35;
        Tooltip = new Tooltip(new ItemTooltipView(Item));
        
        _icon = new Image
        {
            Texture = !string.IsNullOrEmpty(Item.IconHref)
                ? GameService.Content.GetRenderServiceTexture(Item.IconHref)
                : AsyncTexture2D.FromAssetId(1972324),
            Size = new Point(35, 35),
            Tooltip = Tooltip,
            Parent = this
        };

        _name = new Label
        {
            Text = Item.Name,
            Left = 40,
            Width = 385,
            Height = 35,
            VerticalAlignment = VerticalAlignment.Middle,
            TextColor = ItemColors.Rarity(Item.Rarity),
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