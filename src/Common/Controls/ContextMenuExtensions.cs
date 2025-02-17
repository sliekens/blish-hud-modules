using System.Windows.Input;

using Blish_HUD.Controls;

namespace SL.Common.Controls;

public static class ContextMenuExtensions
{
    public static ContextMenuStripItem ToMenuItem(this ICommand command, Func<string> itemText)
    {
        ContextMenuStripItem item = new(itemText());
        item.Click += (_, _) =>
        {
            command.Execute(null);
            item.Text = itemText();
        };
        item.Enabled = command.CanExecute(null);
        command.CanExecuteChanged += (_, _) =>
        {
            item.Enabled = command.CanExecute(null);
            item.Text = itemText();
        };
        return item;
    }
}
