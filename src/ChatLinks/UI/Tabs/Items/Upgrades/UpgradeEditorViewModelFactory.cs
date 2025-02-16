using GuildWars2.Items;

using Microsoft.Extensions.Localization;

using SL.ChatLinks.UI.Tabs.Items.Tooltips;
using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items.Upgrades;

public sealed class UpgradeEditorViewModelFactory(
    IStringLocalizer<UpgradeEditor> localizer,
    IStringLocalizer<UpgradeSlot> localizer2,
    IEventAggregator eventAggregator,
    IClipBoard clipboard,
    ItemIcons icons,
    Customizer customizer,
    UpgradeSelectorViewModelFactory upgradeComponentListViewModelFactory,
    ItemTooltipViewModelFactory itemTooltipViewModelFactory)
{
    public UpgradeEditorViewModel Create(Item targetItem, UpgradeSlotType slotType, UpgradeComponent? defaultUpgradeComponent)
    {
        var upgradeSlotViewModel = new UpgradeSlotViewModel(slotType, icons, localizer2, itemTooltipViewModelFactory, eventAggregator, customizer)
        {
            DefaultUpgradeComponent = defaultUpgradeComponent
        };

        return new UpgradeEditorViewModel(localizer, eventAggregator, clipboard, upgradeSlotViewModel, upgradeComponentListViewModelFactory, targetItem);
    }
}
