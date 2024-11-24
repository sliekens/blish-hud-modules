using Blish_HUD.Content;
using Blish_HUD.Controls;

namespace ChatLinksModule.UI;

public sealed class MainIcon : CornerIcon
{
    public MainIcon()
        : base(AsyncTexture2D.FromAssetId(155156), AsyncTexture2D.FromAssetId(155157), "Chat links")
    {
        Parent = Graphics.SpriteScreen;
        Priority = 745727698;
    }
}