using Blish_HUD.Content;
using Blish_HUD.Controls;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ItemsTab(Func<ItemsTabView> view) : Tab(
    AsyncTexture2D.FromAssetId(156699),
    () => new AsyncView(view()),
    "Items",
    TabPriority.ItemsTab);