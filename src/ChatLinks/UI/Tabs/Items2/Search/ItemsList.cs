using Blish_HUD.Controls;

using GuildWars2.Items;

using SL.Common.Controls;

namespace SL.ChatLinks.UI.Tabs.Items2.Search;

public class ItemsList(ItemsListViewModel viewModel) : ListBox<Item>
{
    protected override Control Template(Item data)
    {
        return new ItemsListEntry(viewModel.CreateListEntryViewModel(data));
    }
}