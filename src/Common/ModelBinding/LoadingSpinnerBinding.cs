using System.Linq.Expressions;

using Blish_HUD.Controls;

namespace SL.Common.ModelBinding;

public sealed class LoadingSpinnerBinding<TViewModel> : ViewModelBinding<TViewModel, bool>
    where TViewModel : ViewModel
{
    public LoadingSpinner LoadingSpinner { get; }

    public LoadingSpinnerBinding(TViewModel viewModel, Expression<Func<TViewModel, bool>> propertySelector, LoadingSpinner loadingSpinner) : base(viewModel, propertySelector)
    {
        LoadingSpinner = loadingSpinner;
        LoadingSpinner.Visible = Snapshot();
    }

    protected override void UpdateView(bool data)
    {
        LoadingSpinner.Visible = data;
    }
}