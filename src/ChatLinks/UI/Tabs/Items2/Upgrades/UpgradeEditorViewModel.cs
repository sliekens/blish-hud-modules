using System.Windows.Input;

using GuildWars2.Items;

using SL.Common;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items2.Upgrades;

public sealed class UpgradeEditorViewModel(
    UpgradeSlotViewModel upgradeSlotViewModel,
    UpgradeSelectorViewModelFactory upgradeComponentListViewModelFactory,
    Item target
) : ViewModel
{
    private bool _customizing;

    public bool Customizing
    {
        get => _customizing;
        private set => SetField(ref _customizing, value);
    }

    public ICommand CustomizeCommand => new RelayCommand(OnCustomize);

    public UpgradeSlotViewModel UpgradeSlotViewModel { get; } = upgradeSlotViewModel;

    public Item TargetItem { get; } = target;

    public ICommand HideCommand => new RelayCommand(OnHide);

    public UpgradeSelectorViewModel CreateUpgradeComponentListViewModel()
    {
        UpgradeSelectorViewModel upgradeComponentListViewModel = upgradeComponentListViewModelFactory.Create(
            TargetItem,
            UpgradeSlotViewModel.Type
        );

        upgradeComponentListViewModel.Selected += Selected;
        upgradeComponentListViewModel.Deselected += Removed;

        return upgradeComponentListViewModel;
    }

    private void Selected(object sender, UpgradeComponent args)
    {
        UpgradeSlotViewModel.SelectedUpgradeComponent = args;
        Customizing = false;
    }

    private void Removed(object sender, EventArgs args)
    {
        UpgradeSlotViewModel.SelectedUpgradeComponent = null;
        Customizing = false;
    }

    private void OnCustomize()
    {
        Customizing = !Customizing;
    }

    private void OnHide()
    {
        Customizing = false;
    }
}