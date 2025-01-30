using System.ComponentModel;

using Blish_HUD;
using Blish_HUD.Controls;

using Microsoft.Xna.Framework;

namespace SL.ChatLinks.UI;

public sealed class MainWindow : TabbedWindow2
{
    private readonly AsyncEmblem _emblem;

    public MainWindow(MainWindowViewModel viewModel) : base(
        viewModel.BackgroundTexture,
        new Rectangle(0, 26, 953, 691),
        new Rectangle(70, 35, 880, 650)
    )
    {
        ViewModel = viewModel;
        _emblem = AsyncEmblem.Attach(this, viewModel.EmblemTexture);
        Parent = Graphics.SpriteScreen;
        Id = viewModel.Id;
        Title = viewModel.Title;
        Location = new Point(300, 300);
        TabChanged += OnTabChanged;
        foreach (var tab in viewModel.Tabs())
        {
            Tabs.Add(tab);
        }

        PropertyChanged += ViewPropertyChanged;
        viewModel.PropertyChanged += ModelPropertyChanged;
        viewModel.Initialize();
    }

    public MainWindowViewModel ViewModel { get; }

    private void ViewPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(Visible):
                ViewModel.Visible = Visible;
                break;
        }
    }

    private void ModelPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(ViewModel.Title):
                Title = ViewModel.Title;
                break;

            case nameof(ViewModel.Visible):
                if (ViewModel.Visible)
                {
                    Show();
                }
                else
                {
                    Hide();
                }

                break;
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
