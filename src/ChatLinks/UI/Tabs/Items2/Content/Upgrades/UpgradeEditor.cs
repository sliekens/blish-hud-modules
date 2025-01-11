using Blish_HUD.Controls;

using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items2.Content.Upgrades;

public sealed class UpgradeEditorViewModel(UpgradeSlotViewModel upgradeSlotViewModel) : ViewModel
{
    public UpgradeSlotViewModel UpgradeSlotViewModel { get; } = upgradeSlotViewModel;
}


public sealed class UpgradeEditor : FlowPanel
{
    public UpgradeEditorViewModel ViewModel { get; }

    private readonly UpgradeSlot _upgradeSlot;

    private Label? _options;

    public UpgradeEditor(UpgradeEditorViewModel viewModel)
    {
        FlowDirection = ControlFlowDirection.SingleTopToBottom;
        WidthSizingMode = SizingMode.AutoSize;
        HeightSizingMode = SizingMode.AutoSize;
        ViewModel = viewModel;
        viewModel.UpgradeSlotViewModel.Customize += Customize;
        _upgradeSlot = CreateUpgradeSlot();
    }

    private UpgradeSlot CreateUpgradeSlot()
    {
        return new UpgradeSlot(ViewModel.UpgradeSlotViewModel)
        {
            Parent = this,
            WidthSizingMode = SizingMode.Fill
        };
    }

    private void Customize(object sender, EventArgs e)
    {
        if (_options is null)
        {
            _options = new Label
            {
                Parent = this,
                Text = "TODO: options",
                AutoSizeWidth = true
            };
        }
        else
        {
            _options?.Dispose();
            _options = null;
        }
    }
}