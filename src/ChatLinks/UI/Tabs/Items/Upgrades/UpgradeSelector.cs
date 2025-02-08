
using System.ComponentModel;

using Blish_HUD.Controls;
using Blish_HUD.Input;

using SL.ChatLinks.UI.Tabs.Items.Collections;
using SL.Common;
using SL.Common.Controls;

namespace SL.ChatLinks.UI.Tabs.Items.Upgrades;

public sealed class UpgradeSelector : FlowPanel
{
    private readonly Accordion _accordion;

    public UpgradeSelectorViewModel ViewModel { get; }


    public UpgradeSelector(UpgradeSelectorViewModel viewModel)
    {
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
        foreach (var group in ViewModel.Options)
        {
            var list = new ItemsList
            {
                Entries = [.. group]
            };

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

    private void MouseEnteredUpgradeSelectorCommand(object sender, MouseEventArgs e)
    {
        ViewModel.MouseEnteredUpgradeSelectorCommand.Execute();
    }

    private void MouseLeftUpgradeSelectorCommand(object sender, MouseEventArgs e)
    {
        ViewModel.MouseLeftUpgradeSelectorCommand.Execute();
    }
}