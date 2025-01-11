using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items2.Content.Upgrades;

public sealed class UpgradeEditorViewModel : ViewModel
{
    public event EventHandler? Customizing;

    public UpgradeEditorViewModel(UpgradeSlotViewModel upgradeSlotViewModel)
    {
        UpgradeSlotViewModel = upgradeSlotViewModel;
        UpgradeSlotViewModel.Customizing += OnCustomizing;
    }

    public UpgradeSlotViewModel UpgradeSlotViewModel { get; }


    private void OnCustomizing(object sender, EventArgs e)
    {
        Customizing?.Invoke(this, EventArgs.Empty);
    }
}