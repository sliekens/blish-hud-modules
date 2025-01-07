using System.Linq.Expressions;

using SL.Common.Controls;

namespace SL.Common.ModelBinding;

public sealed class ListItemBinding<TViewModel, TData> : ViewModelBinding<TViewModel, bool>
    where TViewModel : ViewModel
{
    public ListItem<TData> ListItem { get; }

    public ListItemBinding(TViewModel viewModel, Expression<Func<TViewModel, bool>> propertySelector, ListItem<TData> listItem) : base(viewModel, propertySelector)
    {
        ListItem = listItem;
        listItem.SelectionChanged += SelectionChanged;
        listItem.IsSelected = Snapshot();
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