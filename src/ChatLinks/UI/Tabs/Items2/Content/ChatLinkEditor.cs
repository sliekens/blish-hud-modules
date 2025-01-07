using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;

using Microsoft.Xna.Framework;

using SL.Common.Controls.Items;

using Container = Blish_HUD.Controls.Container;
using ItemTooltipView = SL.ChatLinks.UI.Tabs.Items2.Tooltips.ItemTooltipView;

namespace SL.ChatLinks.UI.Tabs.Items2.Content;

public sealed class ChatLinkEditor : View
{
    public ChatLinkEditorViewModel ViewModel { get; }

    private readonly FlowPanel _layout;

    private readonly ItemImage _itemIcon;

    public ChatLinkEditor(ChatLinkEditorViewModel viewModel)
    {
        ViewModel = viewModel;
        _layout = new FlowPanel()
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
            Width = _layout.Width - 5,
            Height = 50,
            Parent = _layout
        };

        _itemIcon = new ItemImage(ViewModel.Item)
        {
            Parent = header
        };

        _itemIcon.MouseEntered += IconMouseEntered;
    }

    private void IconMouseEntered(object sender, MouseEventArgs e)
    {
        _itemIcon.Tooltip ??= new Tooltip(new ItemTooltipView(ViewModel.CreateTooltipViewModel()));
    }

    protected override void Build(Container buildPanel)
    {
        _layout.Parent = buildPanel;
    }
}