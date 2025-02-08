using GuildWars2.Items;

using Microsoft.Extensions.Localization;

using SL.ChatLinks.UI.Tabs.Items.Tooltips;
using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items.Collections;

public sealed class ItemsListViewModelFactory(
    IStringLocalizer<ItemsList> localizer,
    IEventAggregator eventAggregator,
    IClipBoard clipboard,
    ItemIcons icons,
    Customizer customizer,
    ItemTooltipViewModelFactory tooltipViewModelFactory
)
{
    public ItemsListViewModel Create(Item item, bool isSelected)
    {
        return new ItemsListViewModel(localizer, eventAggregator, clipboard, icons, customizer, item, tooltipViewModelFactory, isSelected);
    }
}