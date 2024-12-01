using Blish_HUD.Controls;

namespace ChatLinksModule.UI.Tabs.Items;

public sealed class ItemDescription : Container
{
    public ItemDescription(string description)
    {
        WidthSizingMode = SizingMode.AutoSize;
        HeightSizingMode = SizingMode.AutoSize;

        FormattedLabel? label = new FormattedLabelBuilder()
            .AddMarkup(description)
            .Wrap()
            .SetWidth(350)
            .AutoSizeHeight()
            .Build();

        label.Parent = this;
    }
}