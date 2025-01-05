using Blish_HUD.Controls;

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
        WidthSizingMode = SizingMode.Fill;
        HeightSizingMode = SizingMode.AutoSize;
        FlowDirection = ControlFlowDirection.SingleLeftToRight;
        _image = new Image
        {
            Parent = this,
            Size = new Point(50),
            Texture = viewModel.GetIcon()
        };

        _name = new Label
        {
            Parent = this,
            Text = Data.Name,
            Width = Width,
            WrapText = true,
            AutoSizeHeight = true,
        };
    }

    public Item Data { get; }

    protected override void OnResized(ResizedEventArgs e)
    {
        base.OnResized(e);
        _name.Width = Width;
    }

    protected override void DisposeControl()
    {
        base.DisposeControl();
        _image.Dispose();
        _name.Dispose();
    }
}