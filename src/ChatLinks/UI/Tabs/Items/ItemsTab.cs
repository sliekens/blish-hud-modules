using Blish_HUD.Content;
using Blish_HUD.Controls;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ItemsTab : Tab
{
    public ItemsTab(Func<ItemsTabView> view)
        : base(AsyncTexture2D.FromAssetId(156699), view, "Items", TabPriority.ItemsTab)
    {
    }
}
