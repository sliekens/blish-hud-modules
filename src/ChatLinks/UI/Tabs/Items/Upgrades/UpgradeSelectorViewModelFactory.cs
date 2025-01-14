using GuildWars2.Items;

using SL.ChatLinks.UI.Tabs.Items.Collections;
using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items.Upgrades;

public sealed class UpgradeSelectorViewModelFactory(
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
            customizer, 
            itemsListViewModelFactory, 
            targetItem, 
            slotType, 
            selectedUpgradeComponent,
            eventAggregator
        );
    }
}