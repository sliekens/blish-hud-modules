using GuildWars2.Items;

using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items2.Search;

public class ItemsListViewModel(ItemsListEntryViewModelFactory factory) : ViewModel
{
    public ItemsListEntryViewModel CreateListEntryViewModel(Item item) => factory.Create(item);
}