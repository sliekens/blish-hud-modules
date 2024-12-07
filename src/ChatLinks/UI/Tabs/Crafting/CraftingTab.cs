using Blish_HUD.Content;
using Blish_HUD.Controls;

namespace SL.ChatLinks.UI.Tabs.Crafting;

public sealed class CraftingTab : Tab
{
    public CraftingTab(Func<CraftingView> view) : base(
        AsyncTexture2D.FromAssetId(156711), view, "Crafting", TabPriority.CraftingTab)
    {
    }
}
