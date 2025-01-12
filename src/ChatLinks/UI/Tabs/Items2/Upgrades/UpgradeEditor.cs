using System.ComponentModel;
using System.Windows.Input;

using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;

using Microsoft.Xna.Framework;

using SL.Common;

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
        viewModel.PropertyChanged += PropertyChanged;

        _upgradeSlot = CreateUpgradeSlot();
        _upgradeSlot.Click += UpgradeSlotClicked;
        _upgradeSlot.Menu = new ContextMenuStrip(() => [
            MenuItem("Customize", ViewModel.CustomizeCommand),
        ]);
    }

    private ContextMenuStripItem MenuItem(string itemText, ICommand command)
    {
        var item = new ContextMenuStripItem(itemText);
        item.Click += (_, _) => command.Execute(null);
        return item;
    }

    private void UpgradeSlotClicked(object sender, MouseEventArgs e)
    {
        Soundboard.Click.Play();
        ViewModel.CustomizeCommand.Execute(null);
    }

    public void ShowOptions()
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

    public void HideOptions()
    {
        _cancelButton?.Dispose();
        _options?.Dispose();
        _cancelButton = null;
        _options = null;
    }

    private void CancelClicked(object sender, MouseEventArgs e)
    {
        ViewModel.HideCommand.Execute(null);
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