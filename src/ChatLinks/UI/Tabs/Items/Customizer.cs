using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;

using SL.ChatLinks.Storage;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class Customizer(
    IDbContextFactory contextFactory,
    ILocale locale
)
{
    public UpgradeComponent? DefaultSuffixItem(Item item)
    {
        return item is not IUpgradable upgradable ? null : GetUpgradeComponent(upgradable.SuffixItemId);
    }

    public async ValueTask<UpgradeComponent?> GetUpgradeComponentAsync(int? upgradeComponentId)
    {
        if (!upgradeComponentId.HasValue)
        {
            return null;
        }

        ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
        await using (context.ConfigureAwait(false))
        {
            return await context.Items
                .OfType<UpgradeComponent>()
                .SingleOrDefaultAsync(item => item.Id == upgradeComponentId.Value).ConfigureAwait(false);
        }
    }

    public UpgradeComponent? GetUpgradeComponent(int? upgradeComponentId)
    {
        if (!upgradeComponentId.HasValue)
        {
            return null;
        }

        using ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
        return context.Items
            .OfType<UpgradeComponent>()
            .SingleOrDefault(item => item.Id == upgradeComponentId.Value);
    }

    public IEnumerable<UpgradeComponent> GetUpgradeComponents(Item targetItem, UpgradeSlotType slotType)
    {
        using ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
        IQueryable<UpgradeComponent> upgrades = slotType switch
        {
            UpgradeSlotType.Infusion => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type = 'upgrade_component'
                      AND InfusionUpgradeFlags -> '$.infusion' = 'true'
                    """
                ),
            UpgradeSlotType.Enrichment => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type = 'upgrade_component'
                      AND InfusionUpgradeFlags -> '$.enrichment' = 'true'
                    """
                ),
            UpgradeSlotType.Default when targetItem is Trinket => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem')
                    	AND UpgradeComponentFlags -> '$.Trinket' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is Backpack => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type = 'gem'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is Armor armor && armor.WeightClass == WeightClass.Heavy => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'rune')
                    	AND UpgradeComponentFlags -> '$.HeavyArmor' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
                ),
            UpgradeSlotType.Default when targetItem is Armor armor && armor.WeightClass == WeightClass.Medium => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'rune')
                    	AND UpgradeComponentFlags -> '$.MediumArmor' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
                ),
            UpgradeSlotType.Default when targetItem is Armor armor && armor.WeightClass == WeightClass.Light => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'rune')
                    	AND UpgradeComponentFlags -> '$.LightArmor' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
                ),
            UpgradeSlotType.Default when targetItem is Axe => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'sigil')
                    	AND UpgradeComponentFlags -> '$.Axe' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is Dagger => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'sigil')
                    	AND UpgradeComponentFlags -> '$.Dagger' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is Focus => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'sigil')
                    	AND UpgradeComponentFlags -> '$.Focus' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is Greatsword => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'sigil')
                    	AND UpgradeComponentFlags -> '$.Greatsword' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is Hammer => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'sigil')
                    	AND UpgradeComponentFlags -> '$.Hammer' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is HarpoonGun => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'sigil')
                    	AND UpgradeComponentFlags -> '$.HarpoonGun' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is Longbow => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'sigil')
                    	AND UpgradeComponentFlags -> '$.LongBow' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is Mace => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'sigil')
                    	AND UpgradeComponentFlags -> '$.Mace' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is Pistol => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'sigil')
                    	AND UpgradeComponentFlags -> '$.Pistol' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is Rifle => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'sigil')
                    	AND UpgradeComponentFlags -> '$.Rifle' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is Scepter => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'sigil')
                    	AND UpgradeComponentFlags -> '$.Scepter' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is Shield => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'sigil')
                    	AND UpgradeComponentFlags -> '$.Shield' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is Shortbow => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'sigil')
                    	AND UpgradeComponentFlags -> '$.ShortBow' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is Spear => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'sigil')
                    	AND UpgradeComponentFlags -> '$.Spear' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is Staff => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'sigil')
                    	AND UpgradeComponentFlags -> '$.Staff' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is Sword => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'sigil')
                    	AND UpgradeComponentFlags -> '$.Sword' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is Torch => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'sigil')
                    	AND UpgradeComponentFlags -> '$.Torch' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is Trident => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'sigil')
                    	AND UpgradeComponentFlags -> '$.Trident' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            UpgradeSlotType.Default when targetItem is Warhorn => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'sigil')
                    	AND UpgradeComponentFlags -> '$.Warhorn' = 'true'
                    	AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    	AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
            ),
            _ => context.Set<UpgradeComponent>()
                .FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type in ('upgrade_component', 'gem', 'rune', 'sigil')
                      AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                      AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """
                    ),
        };

        return [.. upgrades];
    }
}
