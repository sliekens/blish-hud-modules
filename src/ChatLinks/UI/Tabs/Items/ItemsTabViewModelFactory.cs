using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SL.ChatLinks.UI.Tabs.Items.Collections;
using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ItemsTabViewModelFactory(
    ILoggerFactory loggerFactory,
    IStringLocalizer<ItemsTabView> localizer,
    IOptionsMonitor<ChatLinkOptions> options,
    IEventAggregator eventAggregator,
    ItemSearch search,
    ItemsListViewModelFactory itemsListViewModelFactory,
    ChatLinkEditorViewModelFactory chatLinkEditorViewModelFactory)
{
    public ItemsTabViewModel Create()
    {
        return new ItemsTabViewModel(
            loggerFactory.CreateLogger<ItemsTabViewModel>(),
            localizer,
            options,
            eventAggregator,
            search,
            itemsListViewModelFactory,
            chatLinkEditorViewModelFactory
        );
    }
}