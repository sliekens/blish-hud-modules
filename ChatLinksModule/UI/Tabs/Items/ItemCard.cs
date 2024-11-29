using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using Container = Blish_HUD.Controls.Container;

namespace ChatLinksModule.UI.Tabs.Items;

public class ItemCard : Container
{
    public Item Item { get; }

    private readonly Image _image;

    private readonly Label _name;

    public ItemCard(Item item)
    {
        _image = new Image(AsyncTexture2D.FromAssetId(1972324))
        {
            Size = new Point(35, 35),
            Location = new Point(0, 0),
            Parent = this
        };

        if (!string.IsNullOrEmpty(item.IconHref))
        {
            _image.Texture = GameService.Content.GetRenderServiceTexture(item.IconHref);
        }

        _name = new ItemName(item)
        {
            Location = new Point(35, 0),
            Size = new Point(395, 35),
            Parent = this
        };

        Size = new Point(430, 35);
        Item = item;
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
        _image.Dispose();
        _name.Dispose();
        base.DisposeControl();
    }
}