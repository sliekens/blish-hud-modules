using System.ComponentModel;
using System.Linq.Expressions;

using Blish_HUD.Controls;

using SL.Common.Controls;

namespace SL.Common.ModelBinding;

public sealed class Binder
{
    public static ViewModelBinding<TViewModel, TData> Bind<TViewModel, TControl, TData>(
        TViewModel viewModel,
        Expression<Func<TViewModel, TData>> viewModelPropertySelector,
        TControl control,
        Expression<Func<TControl, TData>> controlPropertySelector
    )
        where TViewModel : ViewModel
        where TControl : INotifyPropertyChanged
    {
        return new GenericBinding<TViewModel, TControl, TData>(viewModel, viewModelPropertySelector, control, controlPropertySelector);
    }

    public static ViewModelBinding<TViewModel, string> Bind<TViewModel>(TViewModel viewModel, Expression<Func<TViewModel, string>> propertySelector, TextBox textBox) where TViewModel : ViewModel
    {
        return new TextBoxBinding<TViewModel>(viewModel, propertySelector, textBox);
    }

    public static ViewModelBinding<TViewModel, string> Bind<TViewModel>(TViewModel viewModel, Expression<Func<TViewModel, string>> propertySelector, Label label) where TViewModel : ViewModel
    {
        return new LabelBinding<TViewModel>(viewModel, propertySelector, label);
    }

    public static ViewModelBinding<TViewModel, string> Bind<TViewModel>(TViewModel viewModel, Expression<Func<TViewModel, string>> propertySelector, Scrollbar scrollbar) where TViewModel : ViewModel
    {
        return new ScrollbarTooltipBinding<TViewModel>(viewModel, propertySelector, scrollbar);
    }

    public static ViewModelBinding<TViewModel, int> Bind<TViewModel>(TViewModel viewModel, Expression<Func<TViewModel, int>> propertySelector, NumberInput numberInput) where TViewModel : ViewModel
    {
        return new NumberInputBinding<TViewModel>(viewModel, propertySelector, numberInput);
    }

    public static ViewModelBinding<TViewModel, bool> Bind<TViewModel>(TViewModel viewModel, Expression<Func<TViewModel, bool>> propertySelector, LoadingSpinner loadingSpinner) where TViewModel : ViewModel
    {
        return new LoadingSpinnerBinding<TViewModel>(viewModel, propertySelector, loadingSpinner);
    }

    public static ViewModelBinding<TViewModel, bool> Bind<TViewModel, TData>(TViewModel viewModel, Expression<Func<TViewModel, bool>> propertySelector, ListItem<TData> listItem) where TViewModel : ViewModel
    {
        return new ListItemBinding<TViewModel, TData>(viewModel, propertySelector, listItem);
    }
}