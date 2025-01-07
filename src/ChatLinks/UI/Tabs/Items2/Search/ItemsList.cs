using Blish_HUD.Controls;

using SL.Common.Controls;

namespace SL.ChatLinks.UI.Tabs.Items2.Search;

public class ItemsList : ListBox<ItemsListViewModel>
{
    protected override Control Template(ItemsListViewModel data)
    {
        return new ItemsListEntry(data);
    }
}