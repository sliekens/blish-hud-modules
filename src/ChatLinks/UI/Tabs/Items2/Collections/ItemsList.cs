using Blish_HUD.Controls;

using SL.Common.Controls;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items2.Collections;

public class ItemsList : ListBox<ItemsListViewModel>
{
    protected override Control Template(ItemsListViewModel data)
    {
        ItemsListEntry entry = new(data);
        return entry;
    }

    protected override void Bind(ItemsListViewModel data, ListItem<ItemsListViewModel> listItem)
    {
        Binder.Bind(data, vm => vm.IsSelected, listItem);
    }
}