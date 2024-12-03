using Blish_HUD;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using Container = Blish_HUD.Controls.Container;

namespace ChatLinksModule.UI.Tabs.Items;

public class ItemTooltipView(Item item) : View, ITooltipView
{
    protected override void Build(Container buildPanel)
    {
        FormattedLabelBuilder builder = new FormattedLabelBuilder()
            .Wrap()
            .SetWidth(350)
            .AutoSizeHeight();

        FormattedLabelPartBuilder? namePart = builder.CreatePart(" " + item.Name + "\n");
        if (!string.IsNullOrEmpty(item.IconHref))
        {
            AsyncTexture2D? icon = GameService.Content.GetRenderServiceTexture(item.IconHref);
            namePart.SetPrefixImage(icon)
                .SetPrefixImageSize(new Point(32));
        }

        namePart.SetTextColor(item.Rarity.ToEnum() switch
        {
            Rarity.Junk => new Color(0x99, 0x99, 0x99),
            Rarity.Basic => Color.White,
            Rarity.Fine => new Color(0x44, 0x99, 0xEE),
            Rarity.Masterwork => new Color(0x22, 0xBB, 0x11),
            Rarity.Rare => new Color(0xEE, 0xDD, 0x22),
            Rarity.Exotic => new Color(0xEE, 0x99, 0x00),
            Rarity.Ascended => new Color(0xEE, 0x33, 0x88),
            _ => Color.White
        });
        builder.CreatePart(namePart);

        if (!string.IsNullOrEmpty(item.Description))
        {
            builder.AddMarkup(item.Description);
        }

        if (item.Flags.AccountBound)
        {
            var boundPart = builder.CreatePart("Account Bound on Acquire");
            boundPart.SetFontSize(ContentService.FontSize.Size16);
            builder.CreatePart(boundPart);
        }

        FormattedLabel? label = builder.Build();
        label.Parent = buildPanel;
    }
}