using GuildWars2.Items;

using SL.Common.Controls.Items.Services;

namespace SL.ChatLinks.UI.Tabs.Items2.Tooltips;

public sealed class ItemTooltipViewModelFactory(ItemIcons icons, Customizer customizer)
{
    public ItemTooltipViewModel Create(Item item, int quantity, IEnumerable<UpgradeSlot> upgrades)
    {
        return new ItemTooltipViewModel(item, quantity, upgrades, icons, customizer);
    }
}