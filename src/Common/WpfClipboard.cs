using System.Windows;

using Blish_HUD.Controls;

namespace SL.Common;

public sealed class WpfClipboard : IClipBoard
{
    public void SetText(string value)
    {
        try
        {
            Clipboard.SetText(value);
        }
        catch (Exception e)
        {
            ScreenNotification.ShowNotification(e.Message, ScreenNotification.NotificationType.Error);
        }
    }
}