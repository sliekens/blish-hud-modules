using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;

namespace ChatLinksModule.UI;

public sealed class MainIcon : CornerIcon
{
    private MainIcon(
        AsyncTexture2D icon,
        AsyncTexture2D hoverIcon,
        string iconName)
        : base(icon, hoverIcon, iconName)
    {
    }

    public static MainIcon Create(ModuleParameters parameters)
    {
        var icon = AsyncTexture2D.FromAssetId(155156);
        var hoverIcon = AsyncTexture2D.FromAssetId(155157);
        var iconName = "Chat links";
        return new MainIcon(icon, hoverIcon, iconName)
        {
            Parent = Graphics.SpriteScreen,
            Priority = 745727698
        };
    }
}