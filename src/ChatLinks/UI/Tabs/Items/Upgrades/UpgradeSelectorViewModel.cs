using System.Collections.ObjectModel;

using GuildWars2.Items;

using Microsoft.Extensions.Localization;

using SL.ChatLinks.UI.Tabs.Items.Collections;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items.Upgrades;

public sealed class UpgradeSelectorViewModel : ViewModel, IDisposable
{
    public delegate UpgradeSelectorViewModel Factory(
        Item target,
        UpgradeSlotType slotType,
        UpgradeComponent? selectedUpgradeComponent
    );

    private ObservableCollection<IGrouping<string, ItemsListViewModel>>? _options;

    private readonly IStringLocalizer<UpgradeSelector> _localizer;

    private readonly Customizer _customizer;

    private readonly ItemsListViewModel.Factory _itemsListViewModelFactory;

    private readonly Item _target;

    private readonly UpgradeSlotType _slotType;

    private readonly UpgradeComponent? _selectedUpgradeComponent;

    private readonly IEventAggregator _eventAggregator;

    public UpgradeSelectorViewModel(
        IStringLocalizer<UpgradeSelector> localizer,
        Customizer customizer,
        ItemsListViewModel.Factory itemsListViewModelFactory,
        Item target,
        UpgradeSlotType slotType,
        UpgradeComponent? selectedUpgradeComponent,
        IEventAggregator eventAggregator
)
    {
        ThrowHelper.ThrowIfNull(eventAggregator);
        _localizer = localizer;
        _customizer = customizer;
        _itemsListViewModelFactory = itemsListViewModelFactory;
        _target = target;
        _slotType = slotType;
        _selectedUpgradeComponent = selectedUpgradeComponent;
        _eventAggregator = eventAggregator;
        eventAggregator.Subscribe<LocaleChanged>(OnLocaleChanged);
    }

    private void OnLocaleChanged(LocaleChanged changed)
    {
        Options = null!;
    }

    public event EventHandler<UpgradeSelectedEventArgs>? Selected;

    public event EventHandler? Deselected;

    public ObservableCollection<IGrouping<string, ItemsListViewModel>> Options
    {
        get => _options ??= GetOptions();
        private set => SetField(ref _options, value);
    }

    public IEnumerable<ItemsListViewModel> AllOptions => Options.SelectMany(group => group);

    public RelayCommand<ItemsListViewModel> SelectCommand => new(OnSelect);

    private void OnSelect(ItemsListViewModel selection)
    {
        foreach (ItemsListViewModel option in AllOptions.Where(o => o.IsSelected))
        {
            option.IsSelected = option == selection;
        }

        Selected?.Invoke(this, new()
        {
            Selected = (UpgradeComponent)selection.Item
        });
    }

    public RelayCommand DeselectCommand => new(OnRemove);

    private void OnRemove()
    {
        Deselected?.Invoke(this, EventArgs.Empty);
    }

    public RelayCommand MouseEnteredUpgradeSelectorCommand => new(OnMouseEnteredUpgradeSelector);

    private void OnMouseEnteredUpgradeSelector()
    {
        _eventAggregator.Publish(new MouseEnteredUpgradeSelector());
    }

    public RelayCommand MouseLeftUpgradeSelectorCommand => new(OnMouseLeftUpgradeSelector);

    private void OnMouseLeftUpgradeSelector()
    {
        _eventAggregator.Publish(new MouseLeftUpgradeSelector());
    }

    private ObservableCollection<IGrouping<string, ItemsListViewModel>> GetOptions()
    {
        Dictionary<string, int> groupOrder = new()
        {
            { _localizer["Runes"], 1 },
            { _localizer["Runes (PvP)"], 1 },
            { _localizer["Sigils"], 2 },
            { _localizer["Sigils (PvP)"], 2 },
            { _localizer["Infusions"], 3 },
            { _localizer["Enrichments"], 3 },
            { _localizer["Glyphs"], 4 },
            { _localizer["Jewels"], 5 },
            { _localizer["Universal Upgrades"], 6 },
            { _localizer["Uncategorized"], 7 }
        };

        IOrderedEnumerable<IGrouping<string, ItemsListViewModel>> groups =
            from upgrade in _customizer.GetUpgradeComponents(_target, _slotType)
            let rank = upgrade.Rarity.IsDefined()
                ? upgrade.Rarity.ToEnum() switch
                {
                    Rarity.Junk => 0,
                    Rarity.Basic => 1,
                    Rarity.Fine => 2,
                    Rarity.Masterwork => 3,
                    Rarity.Rare => 4,
                    Rarity.Exotic => 5,
                    Rarity.Ascended => 6,
                    Rarity.Legendary => 7,
                    _ => 99
                }
                : 99
            let vm = _itemsListViewModelFactory(upgrade, upgrade.Id == _selectedUpgradeComponent?.Id)
            orderby rank, upgrade.Level, upgrade.Name
            group vm by (string)(upgrade switch
            {
                Gem => _localizer["Universal Upgrades"],
                Rune when upgrade.GameTypes.All(
                    type => type.IsDefined() && type.ToEnum() is GameType.Pvp or GameType.PvpLobby) => _localizer["Runes (PvP)"],
                Sigil when upgrade.GameTypes.All(
                    type => type.IsDefined() && type.ToEnum() is GameType.Pvp or GameType.PvpLobby) => _localizer["Sigils (PvP)"],
                Rune => _localizer["Runes"],
                Sigil => _localizer["Sigils"],
                _ when upgrade.InfusionUpgradeFlags.Infusion => _localizer["Infusions"],
                _ when upgrade.InfusionUpgradeFlags.Enrichment => _localizer["Enrichments"],
                _ when upgrade.UpgradeComponentFlags.Trinket => _localizer["Jewels"],
                _ when upgrade.UpgradeComponentFlags is
                {
                    LightArmor: false,
                    MediumArmor: false,
                    HeavyArmor: false,
                    Axe: false,
                    Trinket: false
                } => _localizer["Glyphs"],
                _ => _localizer["Uncategorized"]
            })
               into grouped
            orderby groupOrder[grouped.Key]
            select grouped;

        return [.. groups];
    }

    public void Dispose()
    {
        _eventAggregator.Unsubscribe<LocaleChanged>(OnLocaleChanged);
    }
}
