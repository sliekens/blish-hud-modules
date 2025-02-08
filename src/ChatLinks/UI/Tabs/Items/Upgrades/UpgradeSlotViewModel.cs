using Blish_HUD.Content;

using GuildWars2.Items;

using Microsoft.Extensions.Localization;

using SL.ChatLinks.UI.Tabs.Items.Tooltips;
using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items.Upgrades;

public sealed class UpgradeSlotViewModel : ViewModel, IDisposable
{
    private UpgradeSlotType _type;

    private UpgradeComponent? _selectedUpgradeComponent;

    private UpgradeComponent? _defaultUpgradeComponent;

    private readonly ItemIcons _icons;

    private readonly IStringLocalizer<UpgradeSlot> _localizer;

    private readonly ItemTooltipViewModelFactory _itemTooltipViewModelFactory;

    private readonly IEventAggregator _eventAggregator;
    private readonly Customizer _customizer;

    public UpgradeSlotViewModel(
        UpgradeSlotType type,
        ItemIcons icons,
        IStringLocalizer<UpgradeSlot> localizer,
        ItemTooltipViewModelFactory itemTooltipViewModelFactory,
        IEventAggregator eventAggregator,
        Customizer customizer
)
    {
        _icons = icons;
        _localizer = localizer;
        _itemTooltipViewModelFactory = itemTooltipViewModelFactory;
        _eventAggregator = eventAggregator;
        _customizer = customizer;
        _type = type;

        eventAggregator.Subscribe<LocaleChanged>(OnLocaleChanged);
    }

    private async ValueTask OnLocaleChanged(LocaleChanged changed)
    {
        OnPropertyChanged(nameof(EmptySlotTooltip));
        if (SelectedUpgradeComponent is not null)
        {
            var id = SelectedUpgradeComponent.Id;
            SelectedUpgradeComponent = await _customizer.GetUpgradeComponentAsync(id);
        }

        if (DefaultUpgradeComponent is not null)
        {
            var id = DefaultUpgradeComponent.Id;
            DefaultUpgradeComponent = await _customizer.GetUpgradeComponentAsync(id);
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

    public AsyncTexture2D? GetIcon(UpgradeComponent item) => _icons.GetIcon(item);

    public ItemTooltipViewModel CreateTooltipViewModel(UpgradeComponent item) => _itemTooltipViewModelFactory.Create(item, 1, []);

    public void Dispose()
    {
        _eventAggregator.Unsubscribe<LocaleChanged>(OnLocaleChanged);
    }
}