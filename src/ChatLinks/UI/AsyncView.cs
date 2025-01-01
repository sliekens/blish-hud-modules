using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace SL.ChatLinks.UI;

public sealed class AsyncView(Func<IView> other) : View
{
    private readonly ViewContainer _container = new();

    protected override Task<bool> Load(IProgress<string> progress)
    {
        _container.Show(other());
        return Task.FromResult(true);
    }

    protected override void Build(Container buildPanel)
    {
        _container.Parent = buildPanel;
        _container.WidthSizingMode = SizingMode.Fill;
        _container.HeightSizingMode = SizingMode.Fill;
    }
}