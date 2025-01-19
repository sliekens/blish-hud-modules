using System.Linq.Expressions;

using Blish_HUD.Controls;

namespace SL.Common.ModelBinding;

public sealed class ScrollbarTooltipBinding<TViewModel> : ViewModelBinding<TViewModel, string>
    where TViewModel : ViewModel
{
    public Scrollbar Scrollbar { get; }

    public ScrollbarTooltipBinding(TViewModel viewModel, Expression<Func<TViewModel, string>> propertySelector, Scrollbar scrollbar) : base(viewModel, propertySelector)
    {
        Scrollbar = scrollbar;
        scrollbar.BasicTooltipText = Snapshot();
    }

    protected override void UpdateView(string data)
    {
        Scrollbar.BasicTooltipText = data;
    }
}