using System.Windows.Input;

using GuildWars2.Items;

using SL.ChatLinks.UI.Tabs.Items2.Collections;
using SL.Common;
using SL.Common.Controls.Items.Upgrades;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items2.Upgrades;

public sealed class UpgradeSelectorViewModel(
    Customizer customizer,
    ItemsListViewModelFactory itemsListViewModelFactory,
    Item target,
    UpgradeSlotType slotType,
    UpgradeComponent? selectedUpgradeComponent
) : ViewModel
{
    public event EventHandler<UpgradeComponent>? Selected;

    public event EventHandler? Deselected;

    public IReadOnlyList<IGrouping<string, ItemsListViewModel>> Options => _options ??= GetOptions();

    public IEnumerable<ItemsListViewModel> AllOptions => Options.SelectMany(group => group);

    public RelayCommand<ItemsListViewModel> SelectCommand => new(OnSelect);

    public RelayCommand DeselectCommand => new(OnRemove);

    private List<IGrouping<string, ItemsListViewModel>>? _options;

    private void OnSelect(ItemsListViewModel selection)
    {
        foreach (ItemsListViewModel option in AllOptions.Where(o => o.IsSelected))
        {
            option.IsSelected = option == selection;
        }

        Selected?.Invoke(this, (UpgradeComponent)selection.Item);
    }

    private void OnRemove()
    {
        Deselected?.Invoke(this, EventArgs.Empty);
    }

    private List<IGrouping<string, ItemsListViewModel>> GetOptions()
    {
        var groupOrder = new Dictionary<string, int>
        {
            { "Runes", 1 },
            { "Sigils", 1 },
            { "Runes (PvP)", 2 },
            { "Sigils (PvP)", 2 },
            { "Infusions", 3 },
            { "Enrichments", 3 },
            { "Universal Upgrades", 4 },
            { "Uncategorized", 5 }
        };

        var groups =
            from upgrade in customizer.GetUpgradeComponents(target, slotType)
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
            let vm = itemsListViewModelFactory.Create(upgrade, upgrade == selectedUpgradeComponent)
            orderby rank, upgrade.Level, upgrade.Name
            group vm by upgrade switch
            {
                Gem => "Universal Upgrades",
                Rune when upgrade.GameTypes.All(
                    type => type.IsDefined() && type.ToEnum() is GameType.Pvp or GameType.PvpLobby) => "Runes (PvP)",
                Sigil when upgrade.GameTypes.All(
                    type => type.IsDefined() && type.ToEnum() is GameType.Pvp or GameType.PvpLobby) => "Sigils (PvP)",
                Rune => "Runes",
                Sigil => "Sigils",
                _ when upgrade.InfusionUpgradeFlags.Infusion => "Infusions",
                _ when upgrade.InfusionUpgradeFlags.Enrichment => "Enrichments",
                _ => "Uncategorized"
            }
               into grouped
            orderby groupOrder[grouped.Key]
            select grouped;

        return groups.ToList();
    }
}