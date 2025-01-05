using Blish_HUD.Controls;
using Blish_HUD.Input;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.Common.Controls;

namespace SL.ChatLinks.UI.Tabs.Items2;

public sealed class ItemsListEntry : FlowPanel, IListItem<Item>
{
    private readonly Image _image;

    private readonly Label _name;

    public ItemsListEntry(ItemsListEntryViewModel viewModel)
    {
        Data = viewModel.Item;
        Width = 435;
        HeightSizingMode = SizingMode.AutoSize;
        FlowDirection = ControlFlowDirection.SingleLeftToRight;
        _image = new Image
        {
            Parent = this,
            Size = new Point(35),
            Texture = viewModel.GetIcon()
        };

        _name = new Label
        {
            Parent = this,
            Text = Data.Name,
            Width = 400,
            Height = 35,
            WrapText = true,
            VerticalAlignment = VerticalAlignment.Middle
        };
    }

    public Item Data { get; }

    protected override void OnMouseEntered(MouseEventArgs e)
    {
        BackgroundColor = Color.BurlyWood;
        _name.ShowShadow = true;
    }

    protected override void OnMouseLeft(MouseEventArgs e)
    {
        BackgroundColor = Color.Transparent;
        _name.ShowShadow = false;
    }

    protected override void DisposeControl()
    {
        base.DisposeControl();
        _image.Dispose();
        _name.Dispose();
    }
}