using System.Collections.ObjectModel;

using Blish_HUD.Controls;
using Blish_HUD.Input;

using SL.ChatLinks.UI.Tabs.Items2.Collections;
using SL.Common.Controls;

namespace SL.ChatLinks.UI.Tabs.Items2.Upgrades;

public sealed class UpgradeSelector : FlowPanel
{
    public UpgradeSelector(UpgradeSelectorViewModel viewModel)
    {
        WidthSizingMode = SizingMode.Fill;
        HeightSizingMode = SizingMode.AutoSize;
        var accordion = new Accordion
        {
            Parent = this
        };

        foreach (var group in viewModel.GetOptions())
        {
            var list = new ItemsList
            {
                Entries = new ObservableCollection<ItemsListViewModel>(group)
            };

            accordion.AddSection(group.Key, list);

            list.MouseEntered += MouseEnteredList;
            list.MouseLeft += MouseLeftList;
        }
    }

    private void MouseEnteredList(object sender, MouseEventArgs e)
    {
        MessageBus.Send("item editor", "prevent scroll");
    }

    private void MouseLeftList(object sender, MouseEventArgs e)
    {
        MessageBus.Send("item editor", "allow scroll");
    }
}