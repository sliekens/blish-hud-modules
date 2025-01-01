using Blish_HUD.Graphics.UI;

namespace SL.Common;

public static class ViewExtensions
{
    public static void AutoWire<TPresenter>(this View<TPresenter> view)
        where TPresenter : IPresenter
    {
        view.WithPresenter(Objects.Create<TPresenter>(view));
    }
}