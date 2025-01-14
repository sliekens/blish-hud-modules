using GuildWars2.Items;

using SL.ChatLinks.UI.Tabs.Items.Tooltips;
using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items.Upgrades;

public sealed class UpgradeEditorViewModelFactory(
    IEventAggregator eventAggregator,
    IClipBoard clipboard,
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

        return new UpgradeEditorViewModel(eventAggregator, clipboard, upgradeSlotViewModel, upgradeComponentListViewModelFactory, targetItem);
    }
}