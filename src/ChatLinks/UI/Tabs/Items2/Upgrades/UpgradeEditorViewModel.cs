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

    private event EventHandler? CanRemoveChanged;

    public bool Customizing
    {
        get => _customizing;
        set => SetField(ref _customizing, value);
    }

    public RelayCommand CustomizeCommand => new(OnCustomize);

    public RelayCommand RemoveCommand => new(
        OnRemove,
        CanRemove,
        (handler) =>
        {
            CanRemoveChanged += handler;
        },
        handler =>
        {
            CanRemoveChanged -= handler;
        });

    public RelayCommand HideCommand => new(OnHide);

    public UpgradeSlotViewModel UpgradeSlotViewModel { get; } = upgradeSlotViewModel;

    public Item TargetItem { get; } = target;

    public string RemoveItemText =>
        UpgradeSlotViewModel switch
        {
            { SelectedUpgradeComponent: not null } => $"Remove {UpgradeSlotViewModel.SelectedUpgradeComponent.Name}",
            { DefaultUpgradeComponent: not null } => $"Remove {UpgradeSlotViewModel.DefaultUpgradeComponent.Name}",
            _ => "Remove"
        };

    public UpgradeSelectorViewModel CreateUpgradeComponentListViewModel()
    {
        UpgradeSelectorViewModel upgradeComponentListViewModel = upgradeComponentListViewModelFactory.Create(
            TargetItem,
            UpgradeSlotViewModel.Type,
            UpgradeSlotViewModel.SelectedUpgradeComponent
        );

        upgradeComponentListViewModel.Selected += Selected;
        upgradeComponentListViewModel.Deselected += Removed;

        return upgradeComponentListViewModel;
    }

    private void Selected(object sender, UpgradeComponent args)
    {
        UpgradeSlotViewModel.SelectedUpgradeComponent = args;
        Customizing = false;
        CanRemoveChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Removed(object sender, EventArgs args)
    {
        UpgradeSlotViewModel.SelectedUpgradeComponent = null;
        Customizing = false;
        CanRemoveChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnCustomize()
    {
        Customizing = !Customizing;
    }

    private void OnHide()
    {
        Customizing = false;
    }

    private void OnRemove()
    {
        UpgradeSlotViewModel.SelectedUpgradeComponent = null;
        CanRemoveChanged?.Invoke(this, EventArgs.Empty);
    }

    private bool CanRemove()
    {
        return UpgradeSlotViewModel.SelectedUpgradeComponent is not null;
    }

}