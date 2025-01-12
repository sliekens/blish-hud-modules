using GuildWars2.Items;

using SL.ChatLinks.UI.Tabs.Items2.Collections;
using SL.Common.Controls.Items.Upgrades;

namespace SL.ChatLinks.UI.Tabs.Items2.Upgrades;

public sealed class UpgradeSelectorViewModelFactory(Customizer customizer, ItemsListViewModelFactory itemsListViewModelFactory)
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
            selectedUpgradeComponent
        );
    }
}