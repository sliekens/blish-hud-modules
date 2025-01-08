using Blish_HUD.Controls;
using Blish_HUD.Input;

using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items2.Tooltips;

namespace SL.ChatLinks.UI.Tabs.Items2.Search;

public sealed class ItemsListEntry : FlowPanel
{
    private static readonly Color ActiveColor = new Color(109,100,69, 0);
    private static readonly Color HoverColor = new Color(109, 100, 69, 127);
    public ItemsListViewModel ViewModel { get; }

    private readonly Image _image;

    private readonly Panel _labelHolder;

    private readonly Label _name;

    public ItemsListEntry(ItemsListViewModel viewModel)
    {
        ViewModel = viewModel;
        Width = 435;
        HeightSizingMode = SizingMode.AutoSize;
        FlowDirection = ControlFlowDirection.SingleLeftToRight;
        _image = new Image { Parent = this, Size = new Point(35), Texture = viewModel.GetIcon() };

        _labelHolder = new Panel
        {
            Parent = this,
            WidthSizingMode = SizingMode.Fill,
            Height = 35,
            HorizontalScrollOffset = -5
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

    public override void UpdateContainer(GameTime gameTime)
    {
        if (ViewModel.IsSelected)
        {
            _labelHolder.BackgroundColor = ActiveColor;
            _name.ShowShadow = true;
        }
        else if (MouseOver)
        {
            _labelHolder.BackgroundColor = HoverColor;
            _name.ShowShadow = true;
        }
        else
        {
            _labelHolder.BackgroundColor = Color.Transparent;
            _name.ShowShadow = false;
        }

        if (MouseOver)
        {
            _image.Tooltip ??= new Tooltip(new ItemTooltipView(ViewModel.CreateTooltipViewModel()));
            _name.Tooltip ??= new Tooltip(new ItemTooltipView(ViewModel.CreateTooltipViewModel()));
        }

        base.UpdateContainer(gameTime);
    }

    protected override void DisposeControl()
    {
        _image.Dispose();
        _name.Dispose();
        base.DisposeControl();
    }
}