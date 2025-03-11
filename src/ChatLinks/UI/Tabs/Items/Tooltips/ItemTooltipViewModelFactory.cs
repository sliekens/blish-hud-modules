using GuildWars2.Items;

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using SL.ChatLinks.Storage;

namespace SL.ChatLinks.UI.Tabs.Items.Tooltips;

public sealed class ItemTooltipViewModelFactory(
    ILoggerFactory loggerFactory,
    IDbContextFactory contextFactory,
    ILocale locale,
    IconsService icons,
    Customizer customizer,
    AccountUnlocks hero,
    IStringLocalizer<ItemTooltipView> localizer)
{
    public ItemTooltipViewModel Create(Item item, int quantity, IEnumerable<UpgradeSlot> upgrades)
    {
        return new ItemTooltipViewModel(
            loggerFactory.CreateLogger<ItemTooltipViewModel>(),
            contextFactory,
            locale,
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
