namespace SL.Common.Controls;

public sealed class ListBoxSelectionChangedEventArgs<T>(
    IList<ListItem<T>> addedItems,
    IList<ListItem<T>> removedItems
) : EventArgs
{
    public IList<ListItem<T>> AddedItems { get; } = addedItems;

    public IList<ListItem<T>> RemovedItems { get; } = removedItems;
}