using Blish_HUD;
using Blish_HUD.Controls;

using Microsoft.Xna.Framework;

using SL.Common;

namespace SL.ChatLinks.UI;

public sealed class MainWindow : TabbedWindow2
{
    private readonly AsyncEmblem _emblem;

    public MainWindow()
    : this(Objects.Create<MainWindowViewModel>())
    {

    }

    private MainWindow(MainWindowViewModel vm) : base(
        vm.BackgroundTexture,
        new Rectangle(0, 26, 953, 691),
        new Rectangle(70, 71, 839, 605)
    )
    {
        _emblem = AsyncEmblem.Attach(this, vm.EmblemTexture);
        Parent = Graphics.SpriteScreen;
        Id = vm.Id;
        Title = vm.Title;
        Location = new Point(300, 300);
        TabChanged += OnTabChanged;
        foreach (var tab in vm.Tabs())
        {
            Tabs.Add(tab);
        }
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
