
using System.ComponentModel;

using Blish_HUD.Controls;
using Blish_HUD.Input;

using SL.ChatLinks.UI.Tabs.Items.Collections;
using SL.Common.Controls;

namespace SL.ChatLinks.UI.Tabs.Items.Upgrades;

public sealed class UpgradeSelector : FlowPanel
{
    private readonly Accordion _accordion;

    public UpgradeSelectorViewModel ViewModel { get; }


    public UpgradeSelector(UpgradeSelectorViewModel viewModel)
    {
        ThrowHelper.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        WidthSizingMode = SizingMode.Fill;
        HeightSizingMode = SizingMode.AutoSize;
        _accordion = new Accordion
        {
            Parent = this
        };

        AddOptions();

        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void AddOptions()
    {
        foreach (IGrouping<string, ItemsListViewModel> group in ViewModel.Options)
        {
            ItemsList list = new();
            list.SetEntries([.. group]);

            _accordion.AddSection(group.Key, list);

            list.SelectionChanged += SelectionChanged;
            list.MouseEntered += MouseEnteredUpgradeSelectorCommand;
            list.MouseLeft += MouseLeftUpgradeSelectorCommand;
        }
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(UpgradeSelectorViewModel.Options):
                while (_accordion.Children.Count > 0)
                {
                    _accordion.Children[0].Dispose();
                }

                AddOptions();
                break;
            default:
                break;
        }
    }

    private void SelectionChanged(object sender, ListBoxSelectionChangedEventArgs<ItemsListViewModel> args)
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

    private void MouseEnteredUpgradeSelectorCommand(object sender, MouseEventArgs e)
    {
        ViewModel.MouseEnteredUpgradeSelectorCommand.Execute();
    }

    private void MouseLeftUpgradeSelectorCommand(object sender, MouseEventArgs e)
    {
        ViewModel.MouseLeftUpgradeSelectorCommand.Execute();
    }
}
