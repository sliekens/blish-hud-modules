using System.Reflection;

using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;

using GuildWars2.Items;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SL.ChatLinks.UI.Tabs.Items.Controls;

public sealed class ItemsList : FlowPanel
{
    private readonly IReadOnlyDictionary<int, UpgradeComponent> _upgrades;

    private bool _loading;
    
    private static readonly MethodInfo? UpdateScrollbarMethod = typeof(Panel)
        .GetMethod("UpdateScrollbar", BindingFlags.NonPublic | BindingFlags.Instance);

    public event EventHandler<Item>? OptionClicked;

    public ItemsList(IReadOnlyDictionary<int, UpgradeComponent> upgrades)
    {
        _upgrades = upgrades;
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
        Task.Factory.StartNew(state =>
        {
            foreach (var child in (IEnumerable<Control>)state)
            {
                child.Click -= OptionClick;
                child.Dispose();
            }
        }, Children.ToList());

        using var suspend = SuspendLayoutContext();
        ClearChildren();
    }

    public void AddOption(Item item)
    {
        var option = new ItemsListOption(item, _upgrades)
        {
            Parent = this
        };
        option.Click += OptionClick;
    }

    protected override void OnChildAdded(ChildChangedEventArgs e)
    {
        UpdateScrollbar();
        base.OnChildAdded(e);
    }

    protected override void OnChildRemoved(ChildChangedEventArgs e)
    {
        UpdateScrollbar();
        base.OnChildRemoved(e);
    }

    private void UpdateScrollbar()
    {
        UpdateScrollbarMethod?.Invoke(this, []);
    }

    public void RemoveOption(Item item)
    {
        var option = Children.FirstOrDefault(c => c is ItemsListOption option && option.Item == item);
        if (option is null)
        {
            return;
        }

        option.Click -= OptionClick;
        option.Dispose();
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
