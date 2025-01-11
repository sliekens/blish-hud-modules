using Blish_HUD.Controls;

namespace SL.ChatLinks.UI.Tabs.Items2.Content.Upgrades;

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
        _upgradeSlot = CreateUpgradeSlot();
        viewModel.Customizing += OnCustomizing;
    }

    private UpgradeSlot CreateUpgradeSlot()
    {
        return new UpgradeSlot(ViewModel.UpgradeSlotViewModel)
        {
            Parent = this,
            WidthSizingMode = SizingMode.Fill
        };
    }

    private void OnCustomizing(object sender, EventArgs e)
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
            HideOptions();
        }
    }

    public void HideOptions()
    {
        _options?.Dispose();
        _options = null;
    }
}