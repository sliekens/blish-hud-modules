using System.Collections.ObjectModel;

using Blish_HUD.Controls;

using SL.ChatLinks.UI.Tabs.Items2.Search;
using SL.Common.Controls;

namespace SL.ChatLinks.UI.Tabs.Items2.Content.Upgrades;

public sealed class UpgradeComponentList : FlowPanel
{
    public UpgradeComponentList(UpgradeComponentListViewModel viewModel)
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