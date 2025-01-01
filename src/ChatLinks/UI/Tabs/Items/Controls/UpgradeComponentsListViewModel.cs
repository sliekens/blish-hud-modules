using GuildWars2.Items;

using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items.Controls;

public sealed class UpgradeComponentsListViewModel : ViewModel
{
    public event Action<UpgradeComponent>? Selected;

    public IReadOnlyList<UpgradeComponent>? Options { get; set; }

    public void OnSelected(UpgradeComponent upgradeComponent)
    {
        Selected?.Invoke(upgradeComponent);
    }

    public IEnumerable<IGrouping<string, UpgradeComponent>> GetOptions()
    {
        if (Options is null)
        {
            throw new InvalidOperationException();
        }

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

        return from upgrade in Options
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
            orderby rank, upgrade.Level, upgrade.Name
            group upgrade by upgrade switch
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
    }
}