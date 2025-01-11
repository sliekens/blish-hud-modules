using GuildWars2.Items;

using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items2.Upgrades;

public sealed class UpgradeEditorViewModel : ViewModel
{
    public event EventHandler? Customizing;

    private readonly UpgradeSelectorViewModelFactory _upgradeComponentListViewModelFactory;

    public UpgradeEditorViewModel(
        UpgradeSlotViewModel upgradeSlotViewModel,
        UpgradeSelectorViewModelFactory upgradeComponentListViewModelFactory,
        Item target
    )
    {
        _upgradeComponentListViewModelFactory = upgradeComponentListViewModelFactory;
        UpgradeSlotViewModel = upgradeSlotViewModel;
        TargetItem = target;
        UpgradeSlotViewModel.Customizing += OnCustomizing;
    }

    public UpgradeSlotViewModel UpgradeSlotViewModel { get; }

    public Item TargetItem { get; }

    public UpgradeSelectorViewModel CreateUpgradeComponentListViewModel()
    {
        return _upgradeComponentListViewModelFactory.Create(TargetItem, UpgradeSlotViewModel.Type);
    }

    private void OnCustomizing(object sender, EventArgs e)
    {
        Customizing?.Invoke(this, EventArgs.Empty);
    }
}