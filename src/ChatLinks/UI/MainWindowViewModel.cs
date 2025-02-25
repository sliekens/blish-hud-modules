using System.Diagnostics.CodeAnalysis;

using Blish_HUD.Content;

using Microsoft.Extensions.Localization;

using SL.ChatLinks.UI.Tabs.Achievements;
using SL.ChatLinks.UI.Tabs.Items;

namespace SL.ChatLinks.UI;

public sealed class MainWindowViewModel(
    IEventAggregator eventAggregator,
    ItemsTabViewModelFactory itemsTabViewModelFactory,
    AchievementsTabViewModelFactory achievementsTabViewModelFactory,
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
        OnPropertyChanged(nameof(ItemsTabName));
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

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public string Id => "sliekens.chat-links.main-window";

    public string Title => localizer["Title"];

    public string ItemsTabName => localizer["Items"];

    public string AchievementsTabName => localizer["Achievements"];

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public AsyncTexture2D BackgroundTexture => AsyncTexture2D.FromAssetId(155985);

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public AsyncTexture2D EmblemTexture => AsyncTexture2D.FromAssetId(2237584);


    public ItemsTabViewModel CreateItemsTabViewModel()
    {
        return itemsTabViewModelFactory.Create();
    }

    public AchievementsTabViewModel CreateAchievementsTabViewModel()
    {
        return achievementsTabViewModelFactory.Create();
    }

    private void ModuleUnloading(ModuleUnloading obj)
    {
        eventAggregator.Unsubscribe<LocaleChanged>(OnLocaleChanged);
        eventAggregator.Unsubscribe<MainIconClicked>(MainIconClicked);
        eventAggregator.Unsubscribe<ModuleUnloading>(ModuleUnloading);
    }
}
