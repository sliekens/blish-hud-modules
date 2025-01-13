using GuildWars2.Items;

using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.Common;
using SL.Common.Controls.Items.Services;
using SL.Common.Controls.Items.Upgrades;

namespace SL.ChatLinks.UI.Tabs.Items2.Upgrades;

public sealed class UpgradeEditorViewModelFactory(
    IEventAggregator eventAggregator,
    ItemIcons icons,
    Customizer customizer,
    UpgradeSelectorViewModelFactory upgradeComponentListViewModelFactory,
    ItemTooltipViewModelFactory itemTooltipViewModelFactory)
{
    public UpgradeEditorViewModel Create(Item targetItem, UpgradeSlotType slotType, int? defaultUpgradeComponentId)
    {
        UpgradeComponent? defaultUpgradeComponent = null;
        if (defaultUpgradeComponentId.HasValue)
        {
            customizer.UpgradeComponents.TryGetValue(defaultUpgradeComponentId.Value, out defaultUpgradeComponent);
        }

        var upgradeSlotViewModel = new UpgradeSlotViewModel(slotType, icons, itemTooltipViewModelFactory)
        {
            DefaultUpgradeComponent = defaultUpgradeComponent
        };

        return new UpgradeEditorViewModel(eventAggregator, upgradeSlotViewModel, upgradeComponentListViewModelFactory, targetItem);
    }
}