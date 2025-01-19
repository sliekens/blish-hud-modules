using GuildWars2;
using GuildWars2.Items;

using Microsoft.Extensions.Logging;

using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items.Tooltips;

public sealed class ItemTooltipViewModelFactory(
    ILoggerFactory loggerFactory,
    Gw2Client gw2Client,
    ITokenProvider tokenProvider,
    ItemIcons icons,
    Customizer customizer
)
{
    public ItemTooltipViewModel Create(Item item, int quantity, IEnumerable<UpgradeSlot> upgrades)
    {
        return new ItemTooltipViewModel(
            loggerFactory.CreateLogger<ItemTooltipViewModel>(),
            gw2Client, tokenProvider, icons, customizer, item, quantity, upgrades);
    }
}