using Blish_HUD;
using Blish_HUD.Controls;

using GuildWars2.Items;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChatLinksModule.UI.Tabs.Items.Controls;

public class ItemsList : FlowPanel
{
    private bool _loading;

    public ItemsList()
    {
        Size = new Point(450, 500);
        ShowTint = true;
        ShowBorder = true;
        CanScroll = true;
    }

    public void SetLoading(bool loading)
    {
        _loading = loading;
    }

    public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
    {
        base.PaintBeforeChildren(spriteBatch, bounds);
        if (_loading)
        {
            var location = bounds.Center - new Point(32);
            var rect = new Rectangle(location, new Point(64));
            LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, rect);
        }
    }

    public void SetOptions(IEnumerable<Item> items)
    {
        while (!Children.IsEmpty)
        {
            Children[0].Dispose();
        }

        foreach (Item item in items)
        {
            _ = new ItemChoice(item) { Parent = this };
        }
    }
}