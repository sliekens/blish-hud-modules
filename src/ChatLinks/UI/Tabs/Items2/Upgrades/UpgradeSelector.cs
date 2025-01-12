using System.Collections.ObjectModel;

using Blish_HUD.Controls;
using Blish_HUD.Input;

using SL.ChatLinks.UI.Tabs.Items2.Collections;
using SL.Common.Controls;

namespace SL.ChatLinks.UI.Tabs.Items2.Upgrades;

public sealed class UpgradeSelector : FlowPanel
{
    public UpgradeSelectorViewModel ViewModel { get; }

    public UpgradeSelector(UpgradeSelectorViewModel viewModel)
    {
        ViewModel = viewModel;
        WidthSizingMode = SizingMode.Fill;
        HeightSizingMode = SizingMode.AutoSize;
        var accordion = new Accordion
        {
            Parent = this
        };

        foreach (var group in viewModel.Options)
        {
            var list = new ItemsList
            {
                Entries = new ObservableCollection<ItemsListViewModel>(group)
            };

            accordion.AddSection(group.Key, list);

            list.SelectionChanged += SelectionChanged;
            list.MouseEntered += MouseEnteredList;
            list.MouseLeft += MouseLeftList;
        }
    }

    private void SelectionChanged(ListBox<ItemsListViewModel> sender, ListBoxSelectionChangedEventArgs<ItemsListViewModel> args)
    {
        if (args.AddedItems is [{ } item])
        {
            ViewModel.SelectCommand.Execute(item.Data);
        }
        else
        {
            ViewModel.DeselectCommand.Execute();
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