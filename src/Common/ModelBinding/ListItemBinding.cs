using System.Linq.Expressions;

using SL.Common.Controls;

namespace SL.Common.ModelBinding;

public sealed class ListItemBinding<TViewModel, TData> : ViewModelBinding<TViewModel, bool>
    where TViewModel : ViewModel
{
    public ListItem<TData> ListItem { get; }

    public ListItemBinding(
        TViewModel viewModel,
        Expression<Func<TViewModel, bool>> propertySelector,
        ListItem<TData> listItem,
        BindingMode bindingMode
    ) : base(viewModel, propertySelector, bindingMode)
    {
        ThrowHelper.ThrowIfNull(listItem);
        ListItem = listItem;

        if (bindingMode is BindingMode.ToView or BindingMode.Bidirectional)
        {
            UpdateView(Snapshot());
        }

        if (bindingMode is BindingMode.ToModel or BindingMode.Bidirectional)
        {
            listItem.SelectionChanged += SelectionChanged;
        }
    }

    private void SelectionChanged(object sender, EventArgs e)
    {
        UpdateModel(ListItem.IsSelected);
    }

    protected override void UpdateView(bool data)
    {
        ListItem.IsSelected = data;
    }

    protected override void Dispose(bool disposing)
    {
        ListItem.SelectionChanged -= SelectionChanged;
        base.Dispose(disposing);
    }
}
