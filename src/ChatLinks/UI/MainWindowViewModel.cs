using Blish_HUD.Content;
using Blish_HUD.Controls;

using SL.Common;

namespace SL.ChatLinks.UI;

public sealed class MainWindowViewModel(IViewsFactory viewFactory) : ViewModel
{
    public string Id => "sliekens.chat-links.main-window";
    public string Title => "Chat Links";

    public AsyncTexture2D BackgroundTexture => AsyncTexture2D.FromAssetId(155985);

    public AsyncTexture2D EmblemTexture => AsyncTexture2D.FromAssetId(2237584);

    public IEnumerable<Tab> Tabs()
    {
        yield return new Tab(
            AsyncTexture2D.FromAssetId(156699),
            viewFactory.CreateItemsTabView2,
            "Items",
            1);

        yield return new Tab(
            AsyncTexture2D.FromAssetId(156699),
            viewFactory.CreateItemsTabView,
            "Items (old)",
            2);
    }
}