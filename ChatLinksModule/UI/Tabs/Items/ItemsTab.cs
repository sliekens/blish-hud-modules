using Blish_HUD.Content;
using Blish_HUD.Controls;

namespace ChatLinksModule.UI.Tabs.Items;

public sealed class ItemsTab : Tab
{
    public ItemsTab(Func<ItemsView> view)
        : base(AsyncTexture2D.FromAssetId(156699), view, "Items", TabPriority.ItemsTab)
    {
    }
}