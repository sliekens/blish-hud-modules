using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;

using Microsoft.Xna.Framework;

namespace SL.ChatLinks.UI.Tabs.Items2.Upgrades;

public sealed class UpgradeEditor : FlowPanel
{
    public UpgradeEditorViewModel ViewModel { get; }

    private readonly UpgradeSlot _upgradeSlot;

    private StandardButton? _cancelButton;

    private UpgradeSelector? _options;

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
            _cancelButton = new StandardButton
            {
                Parent = this,
                Width = 300,
                Text = "Cancel",
                Icon = AsyncTexture2D.FromAssetId(155149)
            };

            _options = new UpgradeSelector(ViewModel.CreateUpgradeComponentListViewModel())
            {
                Parent = this
            };

            _cancelButton.Click += CancelClicked;
        }
        else
        {
            HideOptions();
        }
    }

    private void CancelClicked(object sender, MouseEventArgs e)
    {
        HideOptions();
    }

    public void HideOptions()
    {
        _cancelButton?.Dispose();
        _options?.Dispose();
        _cancelButton = null;
        _options = null;
    }
}