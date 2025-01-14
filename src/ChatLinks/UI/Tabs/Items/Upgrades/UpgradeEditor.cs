using System.ComponentModel;

using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;

using Microsoft.Xna.Framework;

using SL.Common;
using SL.Common.Controls;

namespace SL.ChatLinks.UI.Tabs.Items.Upgrades;

public sealed class UpgradeEditor : FlowPanel
{
    public UpgradeEditorViewModel ViewModel { get; }

    private readonly UpgradeSlot _upgradeSlot;

    private StandardButton? _cancelButton;

    private UpgradeSelector? _options;

    public UpgradeEditor(UpgradeEditorViewModel viewModel)
    {
        FlowDirection = ControlFlowDirection.SingleTopToBottom;
        Width = 350;
        HeightSizingMode = SizingMode.AutoSize;
        ControlPadding = new Vector2(10);
        ViewModel = viewModel;
        viewModel.PropertyChanged += PropertyChanged;

        _upgradeSlot = CreateUpgradeSlot();
        _upgradeSlot.Click += UpgradeSlotClicked;
        _upgradeSlot.Menu = new ContextMenuStrip(() =>
            [
                ViewModel.CustomizeCommand.ToMenuItem(() => "Customize"),
                ViewModel.RemoveCommand.ToMenuItem(() => ViewModel.RemoveItemText),
                ViewModel.CopyNameCommand.ToMenuItem(() => "Copy Name"),
                ViewModel.CopyChatLinkCommand.ToMenuItem(() => "Copy Chat Link"),
                ViewModel.OpenWikiCommand.ToMenuItem(() => "Open Wiki"),
                ViewModel.OpenApiCommand.ToMenuItem(() => "Open API"),
            ]);
    }

    private void UpgradeSlotClicked(object sender, MouseEventArgs e)
    {
        Soundboard.Click.Play();
        ViewModel.CustomizeCommand.Execute();
    }

    public void ShowOptions()
    {
        _cancelButton = new StandardButton
        {
            Parent = this,
            Width = 350,
            Text = "Cancel",
            Icon = AsyncTexture2D.FromAssetId(155149)
        };

        _options = new UpgradeSelector(ViewModel.CreateUpgradeComponentListViewModel())
        {
            Parent = this
        };

        _cancelButton.Click += CancelClicked;
    }

    public void HideOptions()
    {
        _cancelButton?.Dispose();
        _options?.Dispose();
        _cancelButton = null;
        _options = null;
    }

    private void CancelClicked(object sender, MouseEventArgs e)
    {
        ViewModel.HideCommand.Execute();
    }

    private UpgradeSlot CreateUpgradeSlot()
    {
        return new UpgradeSlot(ViewModel.UpgradeSlotViewModel)
        {
            Parent = this,
            WidthSizingMode = SizingMode.Fill
        };
    }

    private new void PropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(ViewModel.Customizing):
                if (ViewModel.Customizing)
                {
                    ShowOptions();
                }
                else
                {
                    HideOptions();
                }
                break;
        }
    }
}