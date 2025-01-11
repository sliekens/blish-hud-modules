using Blish_HUD.Controls;

using Microsoft.Xna.Framework;

namespace SL.ChatLinks.UI.Tabs.Items2.Content.Upgrades;

public sealed class UpgradeEditor : FlowPanel
{
    public UpgradeEditorViewModel ViewModel { get; }

    private readonly UpgradeSlot _upgradeSlot;

    private UpgradeComponentList? _options;

    public UpgradeEditor(UpgradeEditorViewModel viewModel)
    {
        FlowDirection = ControlFlowDirection.SingleTopToBottom;
        Width = 300;
        HeightSizingMode = SizingMode.AutoSize;
        ControlPadding = new Vector2(10);
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
            _options = new UpgradeComponentList
            {
                Parent = this
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