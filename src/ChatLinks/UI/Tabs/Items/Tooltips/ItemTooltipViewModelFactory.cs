using GuildWars2.Items;

namespace SL.ChatLinks.UI.Tabs.Items.Tooltips;

public sealed class ItemTooltipViewModelFactory(ItemIcons icons, Customizer customizer)
{
    public ItemTooltipViewModel Create(Item item, int quantity, IEnumerable<UpgradeSlot> upgrades)
    {
        return new ItemTooltipViewModel(item, quantity, upgrades, icons, customizer);
    }
}