using Blish_HUD.Controls;
using Blish_HUD.Input;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.Common.Controls;

namespace SL.ChatLinks.UI.Tabs.Items2.Search;

public sealed class ItemsListEntry : FlowPanel, IListItem<Item>
{
    public ItemsListEntryViewModel ViewModel { get; }

    private readonly Image _image;

    private readonly Panel _labelHolder;

    private readonly Label _name;

    public ItemsListEntry(ItemsListEntryViewModel viewModel)
    {
        ViewModel = viewModel;
        Data = viewModel.Item;
        Width = 435;
        HeightSizingMode = SizingMode.AutoSize;
        FlowDirection = ControlFlowDirection.SingleLeftToRight;
        _image = new Image { Parent = this, Size = new Point(35), Texture = viewModel.GetIcon() };

        _labelHolder = new Panel
        {
            Parent = this, WidthSizingMode = SizingMode.Fill, Height = 35, HorizontalScrollOffset = -5
        };

        _name = new Label
        {
            Parent = _labelHolder,
            Text = viewModel.Item.Name,
            TextColor = viewModel.Color,
            Height = 35,
            Width = 395,
            WrapText = true,
            VerticalAlignment = VerticalAlignment.Middle
        };
    }

    public Item Data { get; }

    protected override void OnMouseEntered(MouseEventArgs e)
    {
        _labelHolder.BackgroundColor = Color.BurlyWood;
        _name.ShowShadow = true;
        _image.Tooltip ??= new Tooltip(new ItemTooltipView(ViewModel.CreateTooltipViewModel()));
        _name.Tooltip ??= new Tooltip(new ItemTooltipView(ViewModel.CreateTooltipViewModel()));
        base.OnMouseEntered(e);
    }

    protected override void OnMouseLeft(MouseEventArgs e)
    {
        _labelHolder.BackgroundColor = Color.Transparent;
        _name.ShowShadow = false;
        base.OnMouseLeft(e);
    }

    protected override void DisposeControl()
    {
        _image.Dispose();
        _name.Dispose();
        base.DisposeControl();
    }
}