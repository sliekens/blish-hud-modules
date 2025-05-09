﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;

using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items.Tooltips;
using SL.ChatLinks.UI.Tabs.Items.Upgrades;
using SL.Common.Controls;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ChatLinkEditor : FlowPanel
{
    private readonly Image _itemIcon;

    private readonly Label _itemName;

    private readonly NumberInput _quantity;

    private readonly TrackBar _quantitySlider;

    private readonly TextBox _chatLink;

    private readonly Label _infusionWarning;

    public ChatLinkEditor(ChatLinkEditorViewModel viewModel)
    {
        ThrowHelper.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        FlowDirection = ControlFlowDirection.SingleTopToBottom;
        ControlPadding = new Vector2(0f, 15f);
        OuterControlPadding = new Vector2(20f);
        AutoSizePadding = new Point(10);
        WidthSizingMode = SizingMode.Fill;
        HeightSizingMode = SizingMode.Fill;
        CanScroll = true;
        ShowBorder = true;

        FlowPanel header = new()
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
            Size = new Point(50),
            Menu = new ContextMenuStrip(
                () => [
                    ViewModel.CopyNameCommand.ToMenuItem(() => ViewModel.CopyNameLabel),
                    ViewModel.CopyChatLinkCommand.ToMenuItem(() => viewModel.CopyChatLinkLabel),
                    ViewModel.OpenWikiCommand.ToMenuItem(() => viewModel.OpenWikiLabel),
                    ViewModel.OpenApiCommand.ToMenuItem(() => viewModel.OpenApiLabel),
                ])
        };

        _itemName = new Label
        {
            Parent = header,
            TextColor = viewModel.ItemNameColor,
            Width = 400,
            Height = 50,
            VerticalAlignment = VerticalAlignment.Middle,
            Font = GameService.Content.DefaultFont18,
            WrapText = true,
        };

        _ = Binder.Bind(viewModel, vm => vm.ItemName, _itemName);

        foreach (UpgradeEditorViewModel upgradeEditorViewModel in viewModel.UpgradeEditorViewModels)
        {
            UpgradeEditor editor = new(upgradeEditorViewModel)
            {
                Parent = this
            };
        }

        FlowPanel quantityGroup = new()
        {
            Parent = this,
            FlowDirection = ControlFlowDirection.LeftToRight,
            WidthSizingMode = SizingMode.Fill,
            HeightSizingMode = SizingMode.AutoSize,
            ControlPadding = new Vector2(5f)
        };

        FlowPanel chatLinkGroup = new()
        {
            Parent = this,
            FlowDirection = ControlFlowDirection.LeftToRight,
            WidthSizingMode = SizingMode.Fill,
            HeightSizingMode = SizingMode.AutoSize,
            ControlPadding = new Vector2(5f)
        };

        Label stackSizeLabel = new()
        {
            Parent = quantityGroup,
            AutoSizeWidth = true,
            Height = 32
        };

        _ = Binder.Bind(viewModel, vm => vm.StackSizeLabel, stackSizeLabel);

        _quantity = new NumberInput
        {
            Parent = quantityGroup,
            Width = 80,
            MinValue = 1
        };

        _ = Binder.Bind(viewModel, vm => vm.Quantity, _quantity);
        _ = Binder.Bind(viewModel, vm => vm.MaxStackSize, _quantity, ctl => ctl.MaxValue);

        Panel quantitySliderDiv = new()
        {
            Parent = quantityGroup,
            Width = 80,
            Height = 32
        };

        _quantitySlider = new TrackBar
        {
            Parent = quantitySliderDiv,
            Width = 80,
            Top = 8,
            MinValue = 1
        };

        _ = Binder.Bind(viewModel, vm => vm.Quantity, _quantitySlider, ctl => ctl.Value, BindingMode.Bidirectional);
        _ = Binder.Bind(viewModel, vm => vm.MaxStackSize, _quantitySlider, ctl => ctl.MaxValue);

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
            BasicTooltipText = viewModel.ResetTooltip
        };

        ViewModel.PropertyChanged += (_, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(ViewModel.ResetTooltip):
                    resetQuantity.BasicTooltipText = ViewModel.ResetTooltip;
                    break;
                default:
                    break;
            }
        };

        resetQuantity.Click += ResetQuantityClicked;

        _chatLink = new TextBox
        {
            Parent = chatLinkGroup,
            Width = 350,
            Height = 32
        };

        _ = Binder.Bind(ViewModel, vm => vm.ChatLink, _chatLink, BindingMode.ToView);

        _chatLink.Click += ChatLinkClicked;
        _chatLink.Menu = new ContextMenuStrip(() =>
        [
            viewModel.CopyChatLinkCommand.ToMenuItem(() => viewModel.CopyChatLinkLabel)
        ]);

        GlowButton copyButton = new()
        {
            Parent = chatLinkGroup,
            Icon = AsyncTexture2D.FromAssetId(2208345),
            ActiveIcon = AsyncTexture2D.FromAssetId(2208347)
        };

        copyButton.Click += OnCopyClicked;

        _infusionWarning = new Label
        {
            Parent = this,
            Width = 350,
            AutoSizeHeight = true,
            WrapText = true,
            TextColor = Color.Yellow,
            Visible = ViewModel.ShowInfusionWarning
        };

        _ = Binder.Bind(viewModel, vm => vm.InfusionWarning, _infusionWarning);

        Input.Mouse.MouseWheelScrolled += OnGlobalMouseWheelScrolled;
    }

    private void OnCopyClicked(object sender, MouseEventArgs e)
    {
        Soundboard.Click();
        ViewModel.CopyChatLinkCommand.Execute();
    }

    private void OnGlobalMouseWheelScrolled(object sender, MouseEventArgs e)
    {
        if (_quantitySlider.MouseOver)
        {
            if (Input.Mouse.State.ScrollWheelValue > 0)
            {
                _quantitySlider.Value++;
            }
            else
            {
                _quantitySlider.Value--;
            }
        }
    }

    public ChatLinkEditorViewModel ViewModel { get; }

    public override void UpdateContainer(GameTime gameTime)
    {
        if (_itemIcon.MouseOver)
        {
            _itemIcon.Tooltip ??= new Tooltip(new ItemTooltipView(ViewModel.CreateTooltipViewModel()));
        }
        else
        {
            _itemIcon.Tooltip?.Dispose();
            _itemIcon.Tooltip = null;
        }

        _infusionWarning.Visible = ViewModel.ShowInfusionWarning;
    }

    protected override void OnMouseWheelScrolled(MouseEventArgs e)
    {
        if (ViewModel.AllowScroll)
        {
            base.OnMouseWheelScrolled(e);
        }
    }

    private void MaxQuantityClicked(object sender, MouseEventArgs e)
    {
        Soundboard.Click();
        ViewModel.MaxQuantityCommand.Execute();
    }

    private void ResetQuantityClicked(object sender, MouseEventArgs e)
    {
        Soundboard.Click();
        ViewModel.MinQuantityCommand.Execute();
    }

    private void ChatLinkClicked(object sender, MouseEventArgs e)
    {
        _chatLink.SelectionStart = 0;
        _chatLink.SelectionEnd = _chatLink.Text.Length;
    }

    protected override void DisposeControl()
    {
        Input.Mouse.MouseWheelScrolled -= OnGlobalMouseWheelScrolled;
        ViewModel.Dispose();
        base.DisposeControl();
    }
}
