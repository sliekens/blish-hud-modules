using Blish_HUD.Controls;

using SL.Common;

namespace SL.ChatLinks.UI;

public sealed class MainIcon : CornerIcon
{
    public MainIcon()
    : this(Objects.Create<MainIconViewModel>())
    {
    }

    private MainIcon(MainIconViewModel vm)
        : base(vm.Texture, vm.HoverTexture, vm.Name)
    {
        Parent = Graphics.SpriteScreen;
        Priority = vm.Priority;
    }
}
