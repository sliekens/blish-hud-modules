using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;

using Microsoft.Xna.Framework;

using SL.Common.Controls;
using SL.Common.ModelBinding;

using Container = Blish_HUD.Controls.Container;
using ItemTooltipView = SL.ChatLinks.UI.Tabs.Items2.Tooltips.ItemTooltipView;

namespace SL.ChatLinks.UI.Tabs.Items2.Content;

public sealed class ChatLinkEditor : View
{
    public ChatLinkEditorViewModel ViewModel { get; }

    private readonly FlowPanel _layout;

    private readonly Image _itemIcon;

    private readonly Label _itemName;

    private readonly NumberInput _quantity;

    private readonly TextBox _chatLink;

    public ChatLinkEditor(ChatLinkEditorViewModel viewModel)
    {
        ViewModel = viewModel;
        _layout = new FlowPanel
        {
            ShowTint = true,
            ShowBorder = true,
            FlowDirection = ControlFlowDirection.SingleTopToBottom,
            ControlPadding = new Vector2(0f, 15f),
            OuterControlPadding = new Vector2(10f),
            AutoSizePadding = new Point(10),
            Width = 350,
            HeightSizingMode = SizingMode.Fill,
            CanScroll = true,
        };

        var header = new FlowPanel
        {
            FlowDirection = ControlFlowDirection.SingleLeftToRight,
            ControlPadding = new Vector2(5f),
            WidthSizingMode = SizingMode.Fill,
            Height = 50,
            Parent = _layout
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

        var quantityGroup = new FlowPanel
        {
            Parent = _layout,
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
            //MinValue = 1,
            //MaxValue = 250
        };

        Binder.Bind(viewModel, vm => vm.Quantity, _quantity);

        StandardButton minQuantity = new()
        {
            Parent = quantityGroup,
            Text = "Min",
            Width = 50,
            Height = 32
        };

        minQuantity.Click += MinQuantityOnClick;

        StandardButton maxQuantity = new()
        {
            Parent = quantityGroup,
            Text = "Max",
            Width = 50,
            Height = 32
        };

        maxQuantity.Click += MaxQuantityOnClick;

        _ = new Label { Parent = _layout, Text = "Chat Link:", AutoSizeWidth = true, AutoSizeHeight = true };

        _chatLink = new TextBox
        {
            Parent = _layout,
            Width = 200
        };

        Binder.Bind(ViewModel, vm => vm.ChatLink, _chatLink);

        _chatLink.Click += ChatLinkClicked;
        _chatLink.Menu = new ContextMenuStrip();
        var copy = _chatLink.Menu.AddMenuItem("Copy");
        copy.Click += CopyClicked;
    }

    private void MaxQuantityOnClick(object sender, MouseEventArgs e)
    {
        if (ViewModel.MaxQuantity.CanExecute())
        {
            ViewModel.MaxQuantity.Execute();
        }
    }

    private void MinQuantityOnClick(object sender, MouseEventArgs e)
    {
        if (ViewModel.MinQuantity.CanExecute())
        {
            ViewModel.MinQuantity.Execute();
        }
    }

    private void IconMouseEntered(object sender, MouseEventArgs e)
    {
        _itemIcon.Tooltip ??= new Tooltip(new ItemTooltipView(ViewModel.CreateTooltipViewModel()));
    }

    private void ChatLinkClicked(object sender, MouseEventArgs e)
    {
        _chatLink.SelectionStart = 0;
        _chatLink.SelectionEnd = _chatLink.Text.Length;
    }

    private void CopyClicked(object sender, MouseEventArgs e)
    {
        if (ViewModel.Copy.CanExecute())
        {
            ViewModel.Copy.Execute();
        }
    }

    protected override void Build(Container buildPanel)
    {
        _layout.Parent = buildPanel;
    }
}