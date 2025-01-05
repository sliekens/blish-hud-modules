using Blish_HUD.Controls;

namespace SL.Common.Controls;

public interface IListItem<out TData> : IDisposable
{
    Container Parent { get; set; }

    TData Data { get; }
}