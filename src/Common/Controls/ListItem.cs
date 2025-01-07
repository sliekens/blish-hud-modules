using Blish_HUD.Controls;

namespace SL.Common.Controls;

public sealed class ListItem<TData>(TData data) : Panel
{
    public TData Data { get; } = data;
}