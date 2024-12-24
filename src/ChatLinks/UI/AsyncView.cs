using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace SL.ChatLinks.UI;

public sealed class AsyncView : View
{
    private readonly ViewContainer _container = new();

    public AsyncView(IView other)
    {
        _container.Show(other);
    }

    protected override void Build(Container buildPanel)
    {
        _container.Parent = buildPanel;
        _container.WidthSizingMode = SizingMode.Fill;
        _container.HeightSizingMode = SizingMode.Fill;
    }
}