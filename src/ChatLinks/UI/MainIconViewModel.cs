using Blish_HUD.Content;

using SL.Common;

namespace SL.ChatLinks.UI;

public class MainIconViewModel : ViewModel
{
    public string Name => """
        Chat links
        Right-click for options
        """;

    public AsyncTexture2D Texture => AsyncTexture2D.FromAssetId(155156);

    public AsyncTexture2D HoverTexture => AsyncTexture2D.FromAssetId(155157);

    public int Priority => 745727698;
}