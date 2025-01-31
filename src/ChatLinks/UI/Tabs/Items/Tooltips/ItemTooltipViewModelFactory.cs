using GuildWars2.Items;

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace SL.ChatLinks.UI.Tabs.Items.Tooltips;

public sealed class ItemTooltipViewModelFactory(
    ILoggerFactory loggerFactory,
    ItemIcons icons,
    Customizer customizer,
    Hero hero,
    IStringLocalizer<ItemTooltipViewModel> localizer)
{
    public ItemTooltipViewModel Create(Item item, int quantity, IEnumerable<UpgradeSlot> upgrades)
    {
        return new ItemTooltipViewModel(
            loggerFactory.CreateLogger<ItemTooltipViewModel>(),
            icons,
            customizer,
            hero,
            item,
            quantity,
            upgrades,
            localizer
        );
    }
}