using Microsoft.Extensions.Logging;

using SL.ChatLinks.UI.Tabs.Items.Collections;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ItemsTabViewModelFactory(
    ILoggerFactory loggerFactory,
    ItemSearch search,
    Customizer customizer,
    ItemsListViewModelFactory itemsListViewModelFactory,
    ChatLinkEditorViewModelFactory chatLinkEditorViewModelFactory
)
{
    public ItemsTabViewModel Create()
    {
        return new ItemsTabViewModel(
            loggerFactory.CreateLogger<ItemsTabViewModel>(),
            search,
            customizer,
            itemsListViewModelFactory,
            chatLinkEditorViewModelFactory
        );
    }
}