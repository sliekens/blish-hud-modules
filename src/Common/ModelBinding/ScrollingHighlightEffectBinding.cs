using System.Linq.Expressions;

using Blish_HUD.Controls.Effects;

namespace SL.Common.ModelBinding;

public sealed class ScrollingHighlightEffectBinding<TViewModel> : ViewModelBinding<TViewModel, bool>
    where TViewModel : ViewModel
{
    public ScrollingHighlightEffect ScrollingHighlightEffect { get; }

    public ScrollingHighlightEffectBinding(
        TViewModel viewModel,
        Expression<Func<TViewModel, bool>> propertySelector,
        ScrollingHighlightEffect scrollingHighlightEffect
    ) : base(viewModel, propertySelector, BindingMode.ToView)
    {
        ScrollingHighlightEffect = scrollingHighlightEffect;
        ScrollingHighlightEffect.ForceActive = Snapshot();
    }

    protected override void UpdateView(bool data)
    {
        ScrollingHighlightEffect.ForceActive = data;
    }
}
