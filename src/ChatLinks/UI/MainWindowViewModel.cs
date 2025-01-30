using Blish_HUD.Content;
using Blish_HUD.Controls;

using Microsoft.Extensions.Localization;

using SL.ChatLinks.UI.Tabs.Items;
using SL.Common;

namespace SL.ChatLinks.UI;

public sealed class MainWindowViewModel(
    IEventAggregator eventAggregator,
    ItemsTabViewFactory itemsTabViewFactory,
    IStringLocalizer<MainWindow> localizer) : ViewModel
{
    private bool _visible;

    public void Initialize()
    {
        eventAggregator.Subscribe<LocaleChanged>(OnLocaleChanged);
        eventAggregator.Subscribe<MainIconClicked>(MainIconClicked);
        eventAggregator.Subscribe<ModuleUnloading>(ModuleUnloading);
    }

    private void OnLocaleChanged(LocaleChanged obj)
    {
        OnPropertyChanged(nameof(Title));
    }

    private void MainIconClicked(MainIconClicked obj)
    {
        Visible = !Visible;
    }

    public bool Visible
    {
        get => _visible;
        set => SetField(ref _visible, value);
    }

    public string Id => "sliekens.chat-links.main-window";

    public string Title => localizer["Title"];

    public AsyncTexture2D BackgroundTexture => AsyncTexture2D.FromAssetId(155985);

    public AsyncTexture2D EmblemTexture => AsyncTexture2D.FromAssetId(2237584);

    public IEnumerable<Tab> Tabs()
    {
        yield return new Tab(
            AsyncTexture2D.FromAssetId(156699),
            itemsTabViewFactory.Create,
            "Items",
            1);
    }

    private void ModuleUnloading(ModuleUnloading obj)
    {
        eventAggregator.Unsubscribe<LocaleChanged>(OnLocaleChanged);
        eventAggregator.Unsubscribe<MainIconClicked>(MainIconClicked);
        eventAggregator.Unsubscribe<ModuleUnloading>(ModuleUnloading);
    }
}