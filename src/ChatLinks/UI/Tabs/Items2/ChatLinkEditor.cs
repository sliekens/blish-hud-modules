using System.ComponentModel;

using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;

using Microsoft.Xna.Framework;
using SL.ChatLinks.UI.Tabs.Items2.Upgrades;

using SL.Common.Controls;
using Blish_HUD.Input;

using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.Common.ModelBinding;
using SL.Common;
using SL.Common.Controls.Items.Upgrades;

namespace SL.ChatLinks.UI.Tabs.Items2;

public sealed class ChatLinkEditor : FlowPanel
{
    private readonly Image _itemIcon;

    private readonly Label _itemName;

    private readonly NumberInput _quantity;

    private readonly TextBox _chatLink;

    private readonly ChatLinkEditorViewModel _viewModel;

    private bool _allowScroll = true;

    public ChatLinkEditor(ChatLinkEditorViewModel viewModel)
    {
        _viewModel = viewModel;
        ShowTint = true;
        FlowDirection = ControlFlowDirection.SingleTopToBottom;
        ControlPadding = new Vector2(0f, 15f);
        OuterControlPadding = new Vector2(10f);
        AutoSizePadding = new Point(10);
        Width = 350;
        HeightSizingMode = SizingMode.Fill;
        CanScroll = true;

        var header = new FlowPanel
        {
            FlowDirection = ControlFlowDirection.SingleLeftToRight,
            ControlPadding = new Vector2(5f),
            WidthSizingMode = SizingMode.Fill,
            Height = 50,
            Parent = this
        };

        _itemIcon = new Image
        {
            Parent = header,
            Texture = viewModel.GetIcon(),
            Size = new Point(50)
        };

        _itemIcon.MouseEntered += IconMouseEntered;

        _itemName = new Label
        {
            Parent = header,
            TextColor = viewModel.ItemNameColor,
            Width = 250,
            Height = 50,
            VerticalAlignment = VerticalAlignment.Middle,
            Font = GameService.Content.DefaultFont18,
            WrapText = true,
        };

        Binder.Bind(viewModel, vm => vm.ItemName, _itemName);

        foreach (var upgradeEditorViewModel in viewModel.UpgradeEditorViewModels)
        {
            UpgradeEditor editor = new(upgradeEditorViewModel)
            {
                Parent = this
            };

            upgradeEditorViewModel.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(upgradeEditorViewModel.EffectiveUpgradeComponent):
                        _itemIcon.Tooltip = null;
                        break;
                }
            };
        }

        var quantityGroup = new FlowPanel
        {
            Parent = this,
            FlowDirection = ControlFlowDirection.LeftToRight,
            WidthSizingMode = SizingMode.Fill,
            HeightSizingMode = SizingMode.AutoSize,
            ControlPadding = new Vector2(5f)
        };

        _ = new Label
        {
            Parent = quantityGroup,
            Text = "Stack Size:",
            AutoSizeWidth = true,
            Height = 32
        };

        _quantity = new NumberInput
        {
            Parent = quantityGroup,
            Width = 80,
            Value = 1,
            MinValue = 1,
            MaxValue = 250
        };

        Binder.Bind(viewModel, vm => vm.Quantity, _quantity);

        StandardButton maxQuantity = new()
        {
            Parent = quantityGroup,
            Text = "250",
            Width = 50,
            Height = 32
        };

        maxQuantity.Click += MaxQuantityClicked;

        GlowButton resetQuantity = new()
        {
            Parent = quantityGroup,
            Width = 32,
            Height = 32,
            Icon = AsyncTexture2D.FromAssetId(157324),
            ActiveIcon = AsyncTexture2D.FromAssetId(157325),
            BasicTooltipText = "Reset"
        };

        resetQuantity.Click += ResetQuantityClicked;

        _ = new Label { Parent = this, Text = "Chat Link:", AutoSizeWidth = true, AutoSizeHeight = true };

        _chatLink = new TextBox
        {
            Parent = this,
            Width = 200
        };

        Binder.Bind(_viewModel, vm => vm.ChatLink, _chatLink);

        _chatLink.Click += ChatLinkClicked;
        _chatLink.Menu = new ContextMenuStrip();
        var copy = _chatLink.Menu.AddMenuItem("Copy");
        copy.Click += CopyClicked;

        Label infusionWarning = new()
        {
            Parent = this,
            Width = Width - 20,
            AutoSizeHeight = true,
            WrapText = true,
            TextColor = Color.Yellow,
            Text = """
                   Due to technical restrictions, the game only
                   shows the item's default infusion(s) instead of
                   the selected infusion(s).
                   """,
            Visible = false // TODO: warn when infusions are selected
        };

        MessageBus.Register("item editor", MessageReceived);

        viewModel.PropertyChanged += PropertyChanged;
    }

    private new void PropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(_viewModel.Quantity):
                _itemIcon.Tooltip = null;
                break;
        }
    }

    protected override void OnMouseWheelScrolled(MouseEventArgs e)
    {
        if (_allowScroll)
        {
            base.OnMouseWheelScrolled(e);
        }
    }

    private void MessageReceived(string message)
    {
        switch (message)
        {
            case "prevent scroll":
                _allowScroll = false;
                break;
            case "allow scroll":
                _allowScroll = true;
                break;
        }
    }

    private void MaxQuantityClicked(object sender, MouseEventArgs e)
    {
        Soundboard.Click.Play();
        _viewModel.MaxQuantityCommand.Execute(null);
    }

    private void ResetQuantityClicked(object sender, MouseEventArgs e)
    {
        Soundboard.Click.Play();
        _viewModel.MinQuantityCommand.Execute(null);
    }

    private void IconMouseEntered(object sender, MouseEventArgs e)
    {
        _itemIcon.Tooltip ??= new Tooltip(new ItemTooltipView(_viewModel.CreateTooltipViewModel()));
    }

    private void ChatLinkClicked(object sender, MouseEventArgs e)
    {
        _chatLink.SelectionStart = 0;
        _chatLink.SelectionEnd = _chatLink.Text.Length;
    }

    private void CopyClicked(object sender, MouseEventArgs e)
    {
        _viewModel.CopyCommand.Execute(null);
    }

    protected override void DisposeControl()
    {
        MessageBus.Unregister("item editor");
        base.DisposeControl();
    }
}