using System.Linq.Expressions;

using Blish_HUD.Controls;

namespace SL.Common.ModelBinding;

public sealed class Binder
{
    public static ViewModelBinding<TViewModel, string> Bind<TViewModel>(TViewModel viewModel, Expression<Func<TViewModel, string>> propertySelector, TextBox textBox) where TViewModel : ViewModel
    {
        return new TextBoxBinding<TViewModel>(viewModel, propertySelector, textBox);
    }

    public static ViewModelBinding<TViewModel, bool> Bind<TViewModel>(TViewModel viewModel, Expression<Func<TViewModel, bool>> propertySelector, LoadingSpinner loadingSpinner) where TViewModel : ViewModel
    {
        return new LoadingSpinnerBinding<TViewModel>(viewModel, propertySelector, loadingSpinner);
    }
}