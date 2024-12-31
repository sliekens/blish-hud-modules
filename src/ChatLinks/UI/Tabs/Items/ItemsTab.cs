using Blish_HUD.Content;
using Blish_HUD.Controls;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ItemsTab(IViewsFactory viewFactory) : Tab(
    AsyncTexture2D.FromAssetId(156699),
    viewFactory.CreateItemsTabView,
    "Items",
    TabPriority.ItemsTab);