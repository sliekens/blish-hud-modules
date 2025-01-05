using Blish_HUD.Controls;

namespace SL.Common.Controls;

public sealed class ListItem<TData> : Label, IListItem<TData>
{
    public TData Data { get; }

    public ListItem(TData data)
    {
        Data = data;
        Text = data?.ToString();
    }
}