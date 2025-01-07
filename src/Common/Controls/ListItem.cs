using Blish_HUD.Controls;
using Blish_HUD.Input;

namespace SL.Common.Controls;

public sealed class ListItem<TData>(TData data) : Panel
{
    public event Action<ListItem<TData>, ListItemSelectionChangedEventArgs>? SelectionChanged;

    public TData Data { get; } = data;

    public bool IsSelected { get; set; }

    private readonly object _lock = new();

    protected override void OnClick(MouseEventArgs e)
    {
        lock (_lock)
        {
            IsSelected = !IsSelected;
            SelectionChanged?.Invoke(this, new ListItemSelectionChangedEventArgs
            {
                IsSelected = IsSelected
            });
        }

        base.OnClick(e);
    }

    protected override void DisposeControl()
    {
        SelectionChanged = null;
    }
}