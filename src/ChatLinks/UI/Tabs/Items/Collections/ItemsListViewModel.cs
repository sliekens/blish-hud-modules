using System.Diagnostics;
using System.Net;

using Blish_HUD.Content;

using GuildWars2.Items;

using Microsoft.Extensions.Localization;
using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items.Tooltips;
using SL.Common.ModelBinding;

using UpgradeSlot = SL.ChatLinks.UI.Tabs.Items.Tooltips.UpgradeSlot;

namespace SL.ChatLinks.UI.Tabs.Items.Collections;

public sealed class ItemsListViewModel : ViewModel, IDisposable
{
    private readonly IStringLocalizer<ItemsList> _localizer;

    private readonly IEventAggregator _eventAggregator;

    private readonly IClipBoard _clipboard;

    private readonly IconsService _icons;

    private readonly Customizer _customizer;

    private readonly ItemTooltipViewModelFactory _tooltipViewModelFactory;

    private bool _isSelected;

    public ItemsListViewModel(
        IStringLocalizer<ItemsList> localizer,
        IEventAggregator eventAggregator,
        IClipBoard clipboard,
        IconsService icons,
        Customizer customizer,
        Item item,
        ItemTooltipViewModelFactory tooltipViewModelFactory,
        bool isSelected
    )
    {
        _localizer = localizer;
        _eventAggregator = eventAggregator;
        _clipboard = clipboard;
        _icons = icons;
        _customizer = customizer;
        _tooltipViewModelFactory = tooltipViewModelFactory;
        _isSelected = isSelected;
        Item = item ?? throw new ArgumentNullException(nameof(item));
        Color = ItemColors.Rarity(item.Rarity);
        _eventAggregator.Subscribe<LocaleChanged>(OnLocaleChanged);
    }

    private void OnLocaleChanged(LocaleChanged changed)
    {
        OnPropertyChanged(nameof(SelectLabel));
        OnPropertyChanged(nameof(DeselectLabel));
        OnPropertyChanged(nameof(CopyNameLabel));
        OnPropertyChanged(nameof(CopyChatLinkLabel));
        OnPropertyChanged(nameof(OpenWikiLabel));
        OnPropertyChanged(nameof(OpenApiLabel));
    }

    public Item Item { get; }

    public Color Color { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }

    public string SelectLabel => _localizer["Select"];

    public string DeselectLabel => _localizer["Deselect"];

    public RelayCommand ToggleCommand => new(() => IsSelected = !IsSelected);

    public string CopyNameLabel => _localizer["Copy Name"];

    public RelayCommand CopyNameCommand => new(() => _clipboard.SetText(Item.Name));

    public string CopyChatLinkLabel => _localizer["Copy Chat Link"];

    public RelayCommand CopyChatLinkCommand => new(() => _clipboard.SetText(Item.ChatLink));

    public string OpenWikiLabel => _localizer["Open Wiki"];

    public RelayCommand OpenWikiCommand => new(() => Process.Start(_localizer["Wiki search", WebUtility.UrlEncode(Item.ChatLink)]));

    public string OpenApiLabel => _localizer["Open API"];

    public RelayCommand OpenApiCommand =>
        new(() => Process.Start(_localizer["Item API", Item.Id]));

    public AsyncTexture2D? GetIcon()
    {
        return _icons.GetIcon(Item.IconHref);
    }

    public ItemTooltipViewModel CreateTooltipViewModel()
    {
        List<UpgradeSlot> upgrades = [];
        if (Item is IUpgradable upgradable)
        {
            upgrades.AddRange(UpgradeSlots(upgradable));
            upgrades.AddRange(InfusionSlots(upgradable));
        }

        return _tooltipViewModelFactory.Create(Item, 1, upgrades);
    }

    private IEnumerable<UpgradeSlot> UpgradeSlots(IUpgradable upgradable)
    {
        return upgradable.UpgradeSlots
            .Select(upgradeComponentId => new UpgradeSlot
            {
                Type = UpgradeSlotType.Default,
                UpgradeComponent = _customizer.GetUpgradeComponent(upgradeComponentId)
            });
    }
    private IEnumerable<UpgradeSlot> InfusionSlots(IUpgradable upgradable)
    {
        return upgradable.InfusionSlots
            .Select(infusionSlot => new UpgradeSlot
            {
                Type = infusionSlot.Flags switch
                {
                    { Infusion: true } => UpgradeSlotType.Infusion,
                    { Enrichment: true } => UpgradeSlotType.Enrichment,
                    _ => UpgradeSlotType.Default
                },
                UpgradeComponent = _customizer.GetUpgradeComponent(infusionSlot.ItemId)
            });
    }

    public void Dispose()
    {
        _eventAggregator.Unsubscribe<LocaleChanged>(OnLocaleChanged);
    }
}
