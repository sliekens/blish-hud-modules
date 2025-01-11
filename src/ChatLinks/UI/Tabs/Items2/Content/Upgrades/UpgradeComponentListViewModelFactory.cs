using GuildWars2.Items;

using SL.ChatLinks.UI.Tabs.Items2.Search;
using SL.Common.Controls.Items.Upgrades;

namespace SL.ChatLinks.UI.Tabs.Items2.Content.Upgrades;

public sealed class UpgradeComponentListViewModelFactory(Customizer customizer, ItemsListViewModelFactory itemsListViewModelFactory)
{
    public UpgradeComponentListViewModel Create(Item targetItem, UpgradeSlotType slotType)
    {
        return new UpgradeComponentListViewModel(customizer, itemsListViewModelFactory, targetItem, slotType);
    }
}