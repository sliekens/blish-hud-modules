using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using Blish_HUD.Controls;

namespace SL.Common.Controls;

public class ListBox<T> : FlowPanel
{
    public event Action<ListBox<T>, ListBoxSelectionChangedEventArgs<T>>? SelectionChanged;

    private ObservableCollection<T>? _entries;

    private bool _updatingSelection;

    private readonly ObservableCollection<ListItem<T>> _selection = [];

    public ListBox()
    {
        ShowBorder = true;
        CanScroll = true;

        // ReSharper disable VirtualMemberCallInConstructor
        WidthSizingMode = SizingMode.Fill;
        HeightSizingMode = SizingMode.AutoSize;
        // ReSharper restore VirtualMemberCallInConstructor
    }

    public ObservableCollection<T>? Entries
    {
        get => _entries;
        set
        {
            if (_entries == value) return;
            if (_entries is not null) _entries.CollectionChanged -= EntriesCollectionChanged;
            _entries = value;
            if (value is not null)
            {
                value.CollectionChanged += EntriesCollectionChanged;
                RefreshItems();
            }
        }
    }

    protected virtual void Bind(T data, ListItem<T> listItem)
    {
    }

    protected virtual Control Template(T data)
    {
        return new Label
        {
            Text = data?.ToString(),
            AutoSizeWidth = true,
        };
    }

    protected virtual ListItem<T> AddItem(T data)
    {
        var listItem = new ListItem<T>(data)
        {
            Parent = this,
            ShowTint = Children.Count % 2 == 1
        };

        listItem.Resized += (sender, args) =>
        {
            Invalidate();
        };

        listItem.ContentResized += (sender, args) =>
        {
            Invalidate();
        };

        Bind(data, listItem);

        listItem.SelectionChanged += ListItemSelectionChanged;

        var template = Template(data);
        template.Parent = listItem;
        return listItem;
    }

    private void EntriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (T newItem in e.NewItems)
                {
                    AddItem(newItem);
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                foreach (T oldItem in e.OldItems)
                {
                    RemoveItem(oldItem);
                }
                break;

            case NotifyCollectionChangedAction.Replace:
                foreach (T oldItem in e.OldItems)
                {
                    RemoveItem(oldItem);
                }
                foreach (T newItem in e.NewItems)
                {
                    AddItem(newItem);
                }
                break;

            case NotifyCollectionChangedAction.Move:
                if (e.OldStartingIndex == e.NewStartingIndex)
                {
                    break;
                }

                MoveItems(e.OldItems, e.OldStartingIndex, e.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Reset:
                ClearItems();
                RefreshItems();
                break;
        }

        Invalidate();
    }

    private void ListItemSelectionChanged(ListItem<T> sender, ListItemSelectionChangedEventArgs args)
    {
        try
        {
            // Avoid recursively entering this routine when we update the selection
            if (!_updatingSelection)
            {
                _updatingSelection = true;
                List<ListItem<T>> addedItems = [];
                List<ListItem<T>> removedItems = [];
                if (args.IsSelected)
                {
                    foreach (ListItem<T> previousSelection in _selection)
                    {
                        previousSelection.IsSelected = false;
                        removedItems.Add(previousSelection);
                    }

                    _selection.Clear();
                    _selection.Add(sender);
                    addedItems.Add(sender);
                }
                else
                {
                    _selection.Remove(sender);
                }

                SelectionChanged?.Invoke(
                    this,
                    new ListBoxSelectionChangedEventArgs<T>(addedItems, removedItems)
                );
            }
        }
        finally
        {
            _updatingSelection = false;
        }
    }

    protected virtual void RemoveItem(T item)
    {
        var entry = Children.OfType<ListItem<T>>()
            .FirstOrDefault(child => EqualityComparer<T>.Default.Equals(child.Data, item));
        entry?.Dispose();
    }

    protected virtual void MoveItems(IList children, int oldStartingIndex, int newStartingIndex)
    {
        var collected = new List<Control>(children.Count);

        for (int i = 0; i < children.Count; i++)
        {
            collected.Add(Children[oldStartingIndex]);
            Children.RemoveAt(oldStartingIndex);
        }

        for (int i = 0; i < collected.Count; i++)
        {
            Children.Insert(newStartingIndex + i, collected[i]);
        }

        Invalidate();
    }

    protected virtual void ClearItems()
    {
        while (Children.Count > 0)
        {
            // Assume side-effect of Dispose is removal from Children
            Children[0].Dispose();
        }
    }

    public override void RecalculateLayout()
    {
        this.UpdateScrollbar();
    }

    private void RefreshItems()
    {
        if (_entries is null) return;

        ClearItems();
        foreach (var item in _entries)
        {
            AddItem(item);
        }
    }

    protected override void DisposeControl()
    {
        SelectionChanged = null;
    }
}