using Blish_HUD.Common.UI.Views;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using ChatLinksModule.UI.Tabs.Items.Controls;

using GuildWars2.Items;

using Container = Blish_HUD.Controls.Container;

namespace ChatLinksModule.UI.Tabs.Items;

public class ItemTooltipView(Item item) : View, ITooltipView
{
    protected override void Build(Container buildPanel)
    {
        FlowPanel flowPanel = new()
        {
            Parent = buildPanel,
            WidthSizingMode = SizingMode.AutoSize,
            HeightSizingMode = SizingMode.AutoSize,
            FlowDirection = ControlFlowDirection.SingleTopToBottom
        };

        ItemHeader header = new(item) { Parent = flowPanel };

        if (!string.IsNullOrEmpty(item.Description))
        {
            ItemDescription itemDescription = new(item.Description) { Parent = flowPanel };
        }
    }
}