using Blish_HUD.Controls;

using System.Reflection;

namespace SL.Common.Controls;

internal static class PanelExtensions
{
    public static readonly MethodInfo? UpdateScrollbarMethod = typeof(Panel)
        .GetMethod("UpdateScrollbar", BindingFlags.NonPublic | BindingFlags.Instance);

    public static void UpdateScrollbar(this Panel instance)
    {
        UpdateScrollbarMethod?.Invoke(instance, []);
    }
}