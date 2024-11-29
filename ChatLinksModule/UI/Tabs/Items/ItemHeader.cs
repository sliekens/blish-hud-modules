using Blish_HUD;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Content;
using Blish_HUD.Controls;

using GuildWars2.Items;
using GuildWars2.Markup;

using Microsoft.Xna.Framework;

namespace ChatLinksModule.UI.Tabs.Items;

public sealed class ItemHeader : FlowPanel
{
    public ItemHeader(Item item)
    {
        WidthSizingMode = SizingMode.AutoSize;
        HeightSizingMode = SizingMode.AutoSize;
        Image image = new(AsyncTexture2D.FromAssetId(1972324))
        {
            Size = new Point(50, 50), Location = new Point(0, 0), Parent = this, Padding = new Thickness(5f)
        };

        if (!string.IsNullOrEmpty(item.IconHref))
        {
            image.Texture = GameService.Content.GetRenderServiceTexture(item.IconHref);
        }

        if (!string.IsNullOrEmpty(item.Description))
        {
            image.Tooltip = new Tooltip(new BasicTooltipView(MarkupConverter.ToPlainText(item.Description)));
        }

        ItemName name = new(item)
        {
            Parent = this, AutoSizeWidth = true, Height = 50, VerticalAlignment = VerticalAlignment.Middle
        };

        Item = item;
        FlowDirection = ControlFlowDirection.SingleLeftToRight;
    }

    public Item Item { get; }
}