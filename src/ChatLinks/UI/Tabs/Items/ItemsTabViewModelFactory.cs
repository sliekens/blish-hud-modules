using Microsoft.Extensions.Logging;

using SL.ChatLinks.UI.Tabs.Items.Collections;
using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ItemsTabViewModelFactory(
    ILoggerFactory loggerFactory,
    IEventAggregator eventAggregator,
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
            eventAggregator,
            search,
            customizer,
            itemsListViewModelFactory,
            chatLinkEditorViewModelFactory
        );
    }
}