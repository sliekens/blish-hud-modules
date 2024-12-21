using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;

using GuildWars2.Items;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SL.ChatLinks.UI.Tabs.Items.Controls;

public sealed class ItemsList : FlowPanel
{
    private readonly IDictionary<int, UpgradeComponent> _upgrades;

    private bool _loading;

    public event EventHandler<Item>? OptionClicked;

    public ItemsList(IDictionary<int, UpgradeComponent> upgrades)
    {
        _upgrades = upgrades;
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

    public void ClearOptions()
    {
        while (!Children.IsEmpty)
        {
            Children[0].Click -= OptionClick;
            Children[0].Dispose();
        }
    }

    public void AddOption(Item item)
    {
        var option = new ItemsListOption(item, _upgrades) { Parent = this };
        option.Click += OptionClick;
    }

    public void SetOptions(IEnumerable<Item> items)
    {
        ClearOptions();

        foreach (Item item in items)
        {
            AddOption(item);
        }
    }

    private void OptionClick(object? sender, MouseEventArgs e)
    {
        if (sender is ItemsListOption clickedOption)
        {
            OptionClicked?.Invoke(this, clickedOption.Item);
        }
    }
}
