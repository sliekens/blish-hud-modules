using Microsoft.Extensions.Logging;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ItemsTabViewFactory(
    ILoggerFactory loggerFactory,
    ItemsTabViewModelFactory itemsTabViewModelFactory
)
{
    public ItemsTabView Create()
    {
        ItemsTabViewModel itemsTabViewModel = itemsTabViewModelFactory.Create();
        return new ItemsTabView(loggerFactory.CreateLogger<ItemsTabView>(), itemsTabViewModel);
    }
}