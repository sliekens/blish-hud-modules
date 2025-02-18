using Blish_HUD.Controls;

using SL.Common.Controls;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items.Collections;

public class ItemsList : ListBox<ItemsListViewModel>
{
    protected override Control Template(ItemsListViewModel data)
    {
        ItemsListEntry entry = new(data);
        return entry;
    }

    protected override void Bind(ItemsListViewModel viewModel, ListItem<ItemsListViewModel> listItem)
    {
        ThrowHelper.ThrowIfNull(listItem);
        _ = Binder.Bind(viewModel, vm => vm.IsSelected, listItem);
        listItem.Menu = new ContextMenuStrip(() =>
        [
            viewModel.ToggleCommand.ToMenuItem(() => viewModel.IsSelected ? viewModel.DeselectLabel : viewModel.SelectLabel),
            viewModel.CopyNameCommand.ToMenuItem(() => viewModel.CopyNameLabel),
            viewModel.CopyChatLinkCommand.ToMenuItem(() => viewModel.CopyChatLinkLabel),
            viewModel.OpenWikiCommand.ToMenuItem(() => viewModel.OpenWikiLabel),
            viewModel.OpenApiCommand.ToMenuItem(() => viewModel.OpenApiLabel)
        ]);
    }
}
