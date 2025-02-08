using GuildWars2.Items;

using Microsoft.Extensions.Localization;

using SL.ChatLinks.UI.Tabs.Items.Collections;
using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items.Upgrades;

public sealed class UpgradeSelectorViewModelFactory(
    IStringLocalizer<UpgradeSelector> localizer,
    Customizer customizer,
    ItemsListViewModelFactory itemsListViewModelFactory,
    IEventAggregator eventAggregator)
{
    public UpgradeSelectorViewModel Create(
        Item targetItem,
        UpgradeSlotType slotType,
        UpgradeComponent? selectedUpgradeComponent
    )
    {
        return new UpgradeSelectorViewModel(
            localizer,
            customizer, 
            itemsListViewModelFactory, 
            targetItem, 
            slotType, 
            selectedUpgradeComponent,
            eventAggregator
        );
    }
}