using System.Collections.ObjectModel;

using Blish_HUD.Controls;

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
        }
    }

}