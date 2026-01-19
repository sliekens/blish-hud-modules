using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Controls.Effects;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SL.ChatLinks.UI.Tabs.Items.Tooltips;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items.Collections;

public sealed class ItemsListEntry : Control
{
    private readonly ScrollingHighlightEffect _highlightEffect;

    private readonly AsyncTexture2D? _icon;

    private readonly Rectangle _iconBounds = new(0, 0, 35, 35);
    private readonly ItemsListViewModel _viewModel;
    private Rectangle _textBounds = Rectangle.Empty;

    public ItemsListEntry(ItemsListViewModel viewModel)
    {
        ThrowHelper.ThrowIfNull(viewModel);
        _viewModel = viewModel;
        _icon = viewModel.GetIcon();
        _highlightEffect = new(this);
        EffectBehind = _highlightEffect;
        _ = Binder.Bind(viewModel, viewModel => viewModel.IsSelected, _highlightEffect);

    }

    public override void DoUpdate(GameTime gameTime)
    {
        if (MouseOver)
        {
            Tooltip ??= new Tooltip(new ItemTooltipView(_viewModel.CreateTooltipViewModel()));
        }
        else
        {
            Tooltip?.Dispose();
            Tooltip = null;
        }
    }

    public override void RecalculateLayout()
    {
        Container? parent = Parent;
        if (parent is null)
        {
            return;
        }

        Width = parent.ContentRegion.Width;

        _textBounds = new Rectangle(40, 0, Width - 40, Height);
    }

    protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
    {
        if (_icon is not null)
        {
            spriteBatch.DrawOnCtrl(this, _icon, _iconBounds, Color.White);
        }

        if (MouseOver || _viewModel.IsSelected)
        {
            foreach ((int x, int y) in (ReadOnlySpan<(int, int)>)[(1, 1), (-1, 1), (-1, -1), (1, -1)])
            {
                spriteBatch.DrawStringOnCtrl(
                    this,
                    _viewModel.ItemName,
                    Content.DefaultFont14,
                    _textBounds.OffsetBy(x, y),
                    new Color(Color.Black, .4f),
                    true
                );
            }
        }

        spriteBatch.DrawStringOnCtrl(
            this,
            _viewModel.ItemName,
            Content.DefaultFont14,
            _textBounds,
            _viewModel.Color,
            true
        );
    }
}
