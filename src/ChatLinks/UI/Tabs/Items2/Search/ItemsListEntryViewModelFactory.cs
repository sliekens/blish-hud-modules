using GuildWars2.Items;

using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.Common.Controls.Items.Services;

namespace SL.ChatLinks.UI.Tabs.Items2.Search;

public sealed class ItemsListEntryViewModelFactory(ItemIcons icons, ItemTooltipViewModelFactory tooltipViewModelFactory)
{
    public ItemsListEntryViewModel Create(Item item)
    {
        return new ItemsListEntryViewModel(icons, item, tooltipViewModelFactory);
    }
}