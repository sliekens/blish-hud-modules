using GuildWars2.Items;

using SL.ChatLinks.UI.Tabs.Items.Tooltips;
using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items.Collections;

public sealed class ItemsListViewModelFactory(
    IClipBoard clipboard,
    ItemIcons icons,
    Customizer customizer,
    ItemTooltipViewModelFactory tooltipViewModelFactory
)
{
    public ItemsListViewModel Create(Item item, bool isSelected)
    {
        return new ItemsListViewModel(clipboard, icons, customizer, item, tooltipViewModelFactory, isSelected);
    }
}