using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;

using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items;

namespace SL.ChatLinks.UI;

public sealed class MainWindow : TabbedWindow2
{
    private readonly AsyncEmblem _emblem;

    public MainWindow(ItemsTab items) : base(
        AsyncTexture2D.FromAssetId(155985),
        new Rectangle(0, 26, 953, 691),
        new Rectangle(70, 71, 839, 605)
    )
    {
        Parent = Graphics.SpriteScreen;
        Title = "Chat links";
        Id = "sliekens.chat-links.main-window";
        Location = new Point(300, 300);
        _emblem = AsyncEmblem.Attach(this, AsyncTexture2D.FromAssetId(2237584));
        Tabs.Add(items);
        TabChanged += OnTabChanged;
    }

    private void OnTabChanged(object sender, ValueChangedEventArgs<Tab> args)
    {
        Subtitle = args.NewValue.Name;
    }

    protected override void DisposeControl()
    {
        TabChanged -= OnTabChanged;
        _emblem.Dispose();
        base.DisposeControl();
    }
}
