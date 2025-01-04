using Blish_HUD.Controls;

namespace SL.ChatLinks.UI;

public sealed class MainIcon : CornerIcon
{
    public MainIcon(MainIconViewModel vm)
        : base(vm.Texture, vm.HoverTexture, vm.Name)
    {
        Parent = Graphics.SpriteScreen;
        Priority = vm.Priority;
    }
}
