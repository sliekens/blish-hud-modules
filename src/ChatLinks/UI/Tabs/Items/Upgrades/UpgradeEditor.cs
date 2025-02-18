using System.ComponentModel;

using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;

using Microsoft.Xna.Framework;

using SL.Common.Controls;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items.Upgrades;

public sealed class UpgradeEditor : FlowPanel
{
    public UpgradeEditorViewModel ViewModel { get; }

    private readonly UpgradeSlot _upgradeSlot;

    private StandardButton? _cancelButton;

    private UpgradeSelector? _options;

    public UpgradeEditor(UpgradeEditorViewModel viewModel)
    {
        ThrowHelper.ThrowIfNull(viewModel);
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
                ViewModel.CustomizeCommand.ToMenuItem(() => ViewModel.CustomizeLabel),
                ViewModel.RemoveCommand.ToMenuItem(() => ViewModel.RemoveItemLabel),
                ViewModel.CopyNameCommand.ToMenuItem(() => ViewModel.CopyNameLabel),
                ViewModel.CopyChatLinkCommand.ToMenuItem(() => ViewModel.CopyChatLinkLabel),
                ViewModel.OpenWikiCommand.ToMenuItem(() => ViewModel.OpenWikiLabel),
                ViewModel.OpenApiCommand.ToMenuItem(() => ViewModel.OpenApiLabel),
            ]);
    }

    private void UpgradeSlotClicked(object sender, MouseEventArgs e)
    {
        _ = Soundboard.Click.Play();
        ViewModel.CustomizeCommand.Execute();
    }

    public void ShowOptions()
    {
        _cancelButton = new StandardButton
        {
            Parent = this,
            Width = 350,
            Icon = AsyncTexture2D.FromAssetId(155149)
        };

        _ = Binder.Bind(ViewModel, vm => vm.CancelLabel, _cancelButton, btn => btn.Text);

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
            WidthSizingMode = SizingMode.Fill,
            Opacity = ViewModel.IsCustomizable ? 1f : .33f
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
            default:
                break;
        }
    }

    protected override void DisposeControl()
    {
        base.DisposeControl();
        ViewModel.Dispose();
    }
}
