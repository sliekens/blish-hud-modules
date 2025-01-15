using System.ComponentModel;

using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;

using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items.Tooltips;
using SL.ChatLinks.UI.Tabs.Items.Upgrades;
using SL.Common;
using SL.Common.Controls;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ChatLinkEditor : FlowPanel
{
    private readonly Image _itemIcon;

    private readonly Label _itemName;

    private readonly NumberInput _quantity;

    private readonly TextBox _chatLink;

    private readonly Label _infusionWarning;

    public ChatLinkEditor(ChatLinkEditorViewModel viewModel)
    {
        ViewModel = viewModel;
        FlowDirection = ControlFlowDirection.SingleTopToBottom;
        ControlPadding = new Vector2(0f, 15f);
        OuterControlPadding = new Vector2(20f);
        AutoSizePadding = new Point(10);
        WidthSizingMode = SizingMode.Fill;
        HeightSizingMode = SizingMode.Fill;
        CanScroll = true;
        ShowBorder = true;

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
            Size = new Point(50),
            Menu = new ContextMenuStrip(
                () => [
                    ViewModel.CopyNameCommand.ToMenuItem(() => "Copy Name"),
                    ViewModel.CopyChatLinkCommand.ToMenuItem(() => "Copy Chat Link"),
                    ViewModel.OpenWikiCommand.ToMenuItem(() => "Open Wiki"),
                    ViewModel.OpenApiCommand.ToMenuItem(() => "Open API"),
                ])
        };

        _itemIcon.MouseEntered += IconMouseEntered;

        _itemName = new Label
        {
            Parent = header,
            TextColor = viewModel.ItemNameColor,
            Width = 300,
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

            upgradeEditorViewModel.PropertyChanged += (_, args) =>
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
            MaxValue = ViewModel.MaxStackSize
        };

        ViewModel.PropertyChanged += (_, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(ViewModel.MaxStackSize):
                    _quantity.MaxValue = ViewModel.MaxStackSize;
                    break;
            }
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

        _chatLink = new TextBox
        {
            Parent = this,
            Width = 350
        };

        Binder.Bind(ViewModel, vm => vm.ChatLink, _chatLink);

        _chatLink.Click += ChatLinkClicked;
        _chatLink.Menu = new ContextMenuStrip();
        _chatLink.Menu.AddMenuItem(viewModel.CopyChatLinkCommand.ToMenuItem(() => "Copy"));

        _infusionWarning = new Label
        {
            Parent = this,
            Width = 350,
            AutoSizeHeight = true,
            WrapText = true,
            TextColor = Color.Yellow,
            Text = """
                   Due to technical restrictions, the game only
                   shows the item's default infusion(s) instead of
                   the selected infusion(s).
                   """,
            Visible = ViewModel.ShowInfusionWarning
        };

        viewModel.PropertyChanged += PropertyChanged;
    }

    public ChatLinkEditorViewModel ViewModel { get; }

    public override void UpdateContainer(GameTime gameTime)
    {
        _infusionWarning.Visible = ViewModel.ShowInfusionWarning;
    }

    private new void PropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(ViewModel.Quantity):
                _itemIcon.Tooltip = null;
                break;
        }
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
        Soundboard.Click.Play();
        ViewModel.MaxQuantityCommand.Execute();
    }

    private void ResetQuantityClicked(object sender, MouseEventArgs e)
    {
        Soundboard.Click.Play();
        ViewModel.MinQuantityCommand.Execute();
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
}