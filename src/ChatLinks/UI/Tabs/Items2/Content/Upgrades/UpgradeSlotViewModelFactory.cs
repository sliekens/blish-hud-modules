using GuildWars2.Items;

using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.Common.Controls.Items.Services;
using SL.Common.Controls.Items.Upgrades;

namespace SL.ChatLinks.UI.Tabs.Items2.Content.Upgrades;

public sealed class UpgradeSlotViewModelFactory(
    ItemIcons icons,
    Customizer customizer,
    ItemTooltipViewModelFactory itemTooltipViewModelFactory)
{
    public UpgradeSlotViewModel Create(UpgradeSlotType slotType, int? defaultUpgradeComponentId)
    {
        UpgradeComponent? defaultUpgradeComponent = null;
        if (defaultUpgradeComponentId.HasValue)
        {
            customizer.UpgradeComponents.TryGetValue(defaultUpgradeComponentId.Value, out defaultUpgradeComponent);
        }

        return new UpgradeSlotViewModel(slotType, icons, itemTooltipViewModelFactory)
        {
            DefaultUpgradeComponent = defaultUpgradeComponent
        };
    }
}