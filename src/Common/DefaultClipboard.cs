using System.Windows;

namespace SL.Common;

public sealed class WpfClipboard : IClipBoard
{
    public void SetText(string value)
    {
        Clipboard.SetText(value);
    }
}