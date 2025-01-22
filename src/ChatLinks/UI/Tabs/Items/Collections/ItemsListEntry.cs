using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SL.ChatLinks.UI.Tabs.Items.Tooltips;

namespace SL.ChatLinks.UI.Tabs.Items.Collections;

public sealed class ItemsListEntry(ItemsListViewModel viewModel) : Control
{
    private static readonly Color ActiveColor = new(109, 100, 69, 0);

    private static readonly Color HoverColor = new(109, 100, 69, 127);

    private readonly AsyncTexture2D? _icon = viewModel.GetIcon();

    private readonly Rectangle _iconBounds = new(0, 0, 35, 35);

    private Rectangle _textBounds = Rectangle.Empty;

    public override void DoUpdate(GameTime gameTime)
    {
        BackgroundColor = viewModel.IsSelected switch
        {
            true => ActiveColor,
            false when MouseOver => HoverColor,
            _ => Color.Transparent
        };

        if (MouseOver)
        {
            Tooltip ??= new Tooltip(new ItemTooltipView(viewModel.CreateTooltipViewModel()));
        }
    }

    public override void RecalculateLayout()
    {
        var parent = Parent;
        if (parent is null)
        {
            return;
        }

        Width = parent.ContentRegion.Width;
        Height = 35;

        _textBounds = new Rectangle(40, 0, Width - 40, 35);
    }

    protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
    {
        if (_icon is not null)
        {
            spriteBatch.DrawOnCtrl(this, _icon, _iconBounds, Color.White);
        }

        if (MouseOver || viewModel.IsSelected)
        {
            foreach ((int x, int y) in (ReadOnlySpan<(int, int)>)[(1, 1), (-1, 1), (-1, -1), (1, -1)])
                spriteBatch.DrawStringOnCtrl(
                    this,
                    viewModel.Item.Name,
                    Content.DefaultFont14,
                    _textBounds.OffsetBy(x, y),
                    new Color(Color.Black, .4f),
                    true
                );
        }

        spriteBatch.DrawStringOnCtrl(
            this,
            viewModel.Item.Name,
            Content.DefaultFont14,
            _textBounds,
            viewModel.Color,
            true
        );
    }
}