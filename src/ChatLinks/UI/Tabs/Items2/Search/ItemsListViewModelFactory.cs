using GuildWars2.Items;

using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.Common.Controls.Items.Services;

namespace SL.ChatLinks.UI.Tabs.Items2.Search;

public sealed class ItemsListViewModelFactory(ItemIcons icons, ItemTooltipViewModelFactory tooltipViewModelFactory)
{
    public ItemsListViewModel Create(Item item)
    {
        return new ItemsListViewModel(icons, item, tooltipViewModelFactory);
    }
}