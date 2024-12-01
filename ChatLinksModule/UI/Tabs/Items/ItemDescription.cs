using Blish_HUD.Controls;

using GuildWars2.Markup;

namespace ChatLinksModule.UI.Tabs.Items;

public sealed class ItemDescription : Container
{
    public ItemDescription(string description)
    {
        WidthSizingMode = SizingMode.AutoSize;
        HeightSizingMode = SizingMode.AutoSize;
        Label descriptionLabel = new()
        {
            Parent = this,
            Text = MarkupConverter.ToPlainText(description),
            WrapText = true,
            Width = 350,
            AutoSizeHeight = true,
        };
    }
}