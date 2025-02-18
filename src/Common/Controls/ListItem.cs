using Blish_HUD.Controls;
using Blish_HUD.Input;

namespace SL.Common.Controls;

public sealed class ListItem<TData> : Panel
{
    public event EventHandler<ListItemSelectionChangedEventArgs>? SelectionChanged;

    private readonly object _lock = new();

    private bool _isSelected;

    public ListItem(TData data)
    {
        Data = data;
        WidthSizingMode = SizingMode.Fill;
        HeightSizingMode = SizingMode.AutoSize;
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            lock (_lock)
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    SelectionChanged?.Invoke(this, new ListItemSelectionChangedEventArgs
                    {
                        IsSelected = value
                    });
                }
            }
        }
    }

    protected override void OnContentResized(RegionChangedEventArgs e)
    {
        base.OnContentResized(e);
        foreach (Control child in Children)
        {
            child.Invalidate();
        }
    }

    public TData Data { get; }

    protected override void OnClick(MouseEventArgs e)
    {
        IsSelected = !IsSelected;
        base.OnClick(e);
    }

    protected override void DisposeControl()
    {
        SelectionChanged = null;
    }
}
