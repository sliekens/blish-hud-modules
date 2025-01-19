using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SL.ChatLinks.UI.Tabs.Items.Collections;
using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ItemsTabViewModelFactory(
    ILoggerFactory loggerFactory,
    IOptionsMonitor<ChatLinkOptions> options,
    IEventAggregator eventAggregator,
    ItemSearch search,
    Customizer customizer,
    ItemsListViewModelFactory itemsListViewModelFactory,
    ChatLinkEditorViewModelFactory chatLinkEditorViewModelFactory)
{
    public ItemsTabViewModel Create()
    {
        return new ItemsTabViewModel(
            loggerFactory.CreateLogger<ItemsTabViewModel>(),
            options,
            eventAggregator,
            search,
            customizer,
            itemsListViewModelFactory,
            chatLinkEditorViewModelFactory
        );
    }
}