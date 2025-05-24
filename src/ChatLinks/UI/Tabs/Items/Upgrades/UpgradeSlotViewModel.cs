using Blish_HUD.Content;

using GuildWars2.Items;

using Microsoft.Extensions.Localization;

using SL.ChatLinks.UI.Tabs.Items.Tooltips;

namespace SL.ChatLinks.UI.Tabs.Items.Upgrades;

public sealed class UpgradeSlotViewModel : ViewModel, IDisposable
{
    public delegate UpgradeSlotViewModel Factory(UpgradeSlotType type, UpgradeComponent? defaultUpgradeComponent);

    private UpgradeSlotType _type;

    private UpgradeComponent? _selectedUpgradeComponent;

    private UpgradeComponent? _defaultUpgradeComponent;

    private readonly IconsService _icons;

    private readonly IStringLocalizer<UpgradeSlot> _localizer;

    private readonly ItemTooltipViewModel.Factory _itemTooltipViewModelFactory;

    private readonly IEventAggregator _eventAggregator;

    private readonly Customizer _customizer;

    public UpgradeSlotViewModel(
        IconsService icons,
        IStringLocalizer<UpgradeSlot> localizer,
        ItemTooltipViewModel.Factory itemTooltipViewModelFactory,
        IEventAggregator eventAggregator,
        Customizer customizer,
        UpgradeSlotType type,
        UpgradeComponent? defaultUpgradeComponent
)
    {
        ThrowHelper.ThrowIfNull(eventAggregator);
        _icons = icons;
        _localizer = localizer;
        _itemTooltipViewModelFactory = itemTooltipViewModelFactory;
        _eventAggregator = eventAggregator;
        _customizer = customizer;
        _type = type;
        _defaultUpgradeComponent = defaultUpgradeComponent;

        eventAggregator.Subscribe<LocaleChanged>(OnLocaleChanged);
    }

    private async Task OnLocaleChanged(LocaleChanged changed)
    {
        OnPropertyChanged(nameof(EmptySlotTooltip));
        if (SelectedUpgradeComponent is not null)
        {
            int id = SelectedUpgradeComponent.Id;
            SelectedUpgradeComponent = await _customizer.GetUpgradeComponentAsync(id).ConfigureAwait(false);
        }

        if (DefaultUpgradeComponent is not null)
        {
            int id = DefaultUpgradeComponent.Id;
            DefaultUpgradeComponent = await _customizer.GetUpgradeComponentAsync(id).ConfigureAwait(false);
        }
        else
        {
            OnPropertyChanged(nameof(DefaultUpgradeComponent));
        }
    }

    public UpgradeSlotType Type
    {
        get => _type;
        set => SetField(ref _type, value);
    }

    public UpgradeComponent? DefaultUpgradeComponent
    {
        get => _defaultUpgradeComponent;
        set => SetField(ref _defaultUpgradeComponent, value);
    }

    public UpgradeComponent? SelectedUpgradeComponent
    {
        get => _selectedUpgradeComponent;
        set => SetField(ref _selectedUpgradeComponent, value);
    }

    public string EmptySlotTooltip => _localizer["Empty slot tooltip"];

    public string UnusedUpgradeSlotLabel => _localizer["Unused upgrade slot"];

    public string UnusedInfusionSlotLabel => _localizer["Unused infusion slot"];

    public string UnusedEnrichmenSlotLabel => _localizer["Unused enrichment slot"];

    public AsyncTexture2D? GetIcon(UpgradeComponent item)
    {
        ThrowHelper.ThrowIfNull(item);
        return _icons.GetIcon(item.IconUrl);
    }

    public ItemTooltipViewModel CreateTooltipViewModel(UpgradeComponent item)
    {
        return _itemTooltipViewModelFactory(item, 1, []);
    }

    public void Dispose()
    {
        _eventAggregator.Unsubscribe<LocaleChanged>(OnLocaleChanged);
    }
}
