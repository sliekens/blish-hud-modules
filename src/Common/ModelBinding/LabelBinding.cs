using System.Linq.Expressions;

using Blish_HUD.Controls;

namespace SL.Common.ModelBinding;

public sealed class LabelBinding<TViewModel> : ViewModelBinding<TViewModel, string>
    where TViewModel : ViewModel
{
    public Label Label { get; }

    public LabelBinding(
        TViewModel viewModel,
        Expression<Func<TViewModel, string>> propertySelector,
        Label label
    ) : base(viewModel, propertySelector, BindingMode.ToView)
    {
        Label = label;
        label.Text = Snapshot();
    }

    protected override void UpdateView(string data)
    {
        Label.Text = data;
    }
}
