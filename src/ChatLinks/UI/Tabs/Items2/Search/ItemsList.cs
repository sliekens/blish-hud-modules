using GuildWars2.Items;

using SL.Common.Controls;

namespace SL.ChatLinks.UI.Tabs.Items2.Search;

public class ItemsList(ItemsListViewModel viewModel) : ListBox<Item>
{
    protected override IListItem<Item> Template(Item item)
    {
        return new ItemsListEntry(viewModel.CreateListEntryViewModel(item));
    }

    protected override IListItem<Item> AddItem(Item item)
    {
        var listItem = (ItemsListEntry)base.AddItem(item);
        if (Children.IndexOf(listItem) % 2 == 0)
        {
            listItem.ShowTint = true;
        }

        return listItem;
    }
}