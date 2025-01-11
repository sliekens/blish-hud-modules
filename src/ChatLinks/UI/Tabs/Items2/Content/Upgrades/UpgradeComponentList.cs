using Blish_HUD.Controls;

using SL.Common.Controls;

namespace SL.ChatLinks.UI.Tabs.Items2.Content.Upgrades;

public sealed class UpgradeComponentList : FlowPanel
{
    public UpgradeComponentList()
    {
        WidthSizingMode = SizingMode.Fill;
        HeightSizingMode = SizingMode.AutoSize;
        var container = new Accordion
        {
            Parent = this
        };

        container.AddSection("First", new Label
        {
            Text = "TODO TODO TODO",
            AutoSizeWidth = true
        });
        container.AddSection("Second", new Label
        {
            Text = "TODO TODO TODO TODO TODO",
            AutoSizeWidth = true
        });
        container.AddSection("Third", new Label
        {
            Text = "TODO TODO TODO",
            AutoSizeWidth = true
        });
    }

}