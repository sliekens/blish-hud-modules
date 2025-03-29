using System.Runtime.CompilerServices;

using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;

using SL.ChatLinks.Storage;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ItemsFilter
{
    public string? Label { get; set; }

    public string? Category { get; set; }

    public string? Text { get; set; }
}

public sealed record ResultContext
{
    public int ResultTotal { get; set; }
}

public sealed class ItemSearch(IDbContextFactory contextFactory, ILocale locale)
{
    public async ValueTask<int> CountItems()
    {
        ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
        await using (context.ConfigureAwait(false))
        {
            return await context.Items.CountAsync().ConfigureAwait(false);
        }
    }

    public async IAsyncEnumerable<Item> NewItems(int limit)
    {
        ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
        await using (context.ConfigureAwait(false))
        {
            await foreach (Item? item in context.Items
                               .OrderByDescending(item => item.Id)
                               .Take(limit)
                               .AsAsyncEnumerable().ConfigureAwait(false))
            {
                yield return item;
            }
        }
    }

    public async IAsyncEnumerable<Item> FilterItems(
        ItemsFilter filter,
        int limit,
        ResultContext resultContext,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        ThrowHelper.ThrowIfNull(filter);
        ThrowHelper.ThrowIfNull(resultContext);
        ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
        await using (context.ConfigureAwait(false))
        {
            IQueryable<Item> query = filter.Category switch
            {
                "armor" => context.Items.OfType<Armor>(),
                "chest" => context.Items.OfType<Coat>(),
                "leggings" => context.Items.OfType<Leggings>(),
                "gloves" => context.Items.OfType<Gloves>(),
                "helm" => context.Items.OfType<Helm>(),
                "helm_aquatic" => context.Items.OfType<HelmAquatic>(),
                "boots" => context.Items.OfType<Boots>(),
                "shoulders" => context.Items.OfType<Shoulders>(),
                "back" => context.Items.OfType<Backpack>(),
                "trinket" => context.Items.OfType<Trinket>(),
                "accessory" => context.Items.OfType<Accessory>(),
                "amulet" => context.Items.OfType<Amulet>(),
                "ring" => context.Items.OfType<Ring>(),
                "weapon" => context.Items.OfType<Weapon>(),
                "axe" => context.Items.OfType<Axe>(),
                "dagger" => context.Items.OfType<Dagger>(),
                "focus" => context.Items.OfType<Focus>(),
                "greatsword" => context.Items.OfType<Greatsword>(),
                "hammer" => context.Items.OfType<Hammer>(),
                "harpoon_gun" => context.Items.OfType<HarpoonGun>(),
                "large_bundle" => context.Items.OfType<LargeBundle>(),
                "longbow" => context.Items.OfType<Longbow>(),
                "mace" => context.Items.OfType<Mace>(),
                "pistol" => context.Items.OfType<Pistol>(),
                "rifle" => context.Items.OfType<Rifle>(),
                "scepter" => context.Items.OfType<Scepter>(),
                "shield" => context.Items.OfType<Shield>(),
                "shortbow" => context.Items.OfType<Shortbow>(),
                "small_bundle" => context.Items.OfType<SmallBundle>(),
                "spear" => context.Items.OfType<Spear>(),
                "staff" => context.Items.OfType<Staff>(),
                "sword" => context.Items.OfType<Sword>(),
                "torch" => context.Items.OfType<Torch>(),
                "toy" => context.Items.OfType<Toy>(),
                "toy_two_handed" => context.Items.OfType<ToyTwoHanded>(),
                "trident" => context.Items.OfType<Trident>(),
                "warhorn" => context.Items.OfType<Warhorn>(),
                "consumable" => context.Items.OfType<Consumable>(),
                "appearance_changer" => context.Items.OfType<AppearanceChanger>(),
                "booze" => context.Items.OfType<Booze>(),
                "contract_npc" => context.Items.OfType<ContractNpc>(),
                "currency" => context.Items.OfType<Currency>(),
                "food" => context.Items.OfType<Food>(),
                "generic_consumable" => context.Items.OfType<GenericConsumable>(),
                "halloween_consumable" => context.Items.OfType<HalloweenConsumable>(),
                "mount_license" => context.Items.OfType<MountLicense>(),
                "random_unlocker" => context.Items.OfType<RandomUnlocker>(),
                "service" => context.Items.OfType<Service>(),
                "teleport_to_friend" => context.Items.OfType<TeleportToFriend>(),
                "transmutation" => context.Items.OfType<Transmutation>(),
                "unlocker" => context.Items.OfType<Unlocker>(),
                "bag_slot_expansion" => context.Items.OfType<BagSlotExpansion>(),
                "bank_tab_expansion" => context.Items.OfType<BankTabExpansion>(),
                "build_storage_expansion" => context.Items.OfType<BuildStorageExpansion>(),
                "build_template_expansion" => context.Items.OfType<BuildTemplateExpansion>(),
                "content_unlocker" => context.Items.OfType<ContentUnlocker>(),
                "dye" => context.Items.OfType<Dye>(),
                "equipment_template_expansion" => context.Items.OfType<EquipmentTemplateExpansion>(),
                "glider_skin_unlocker" => context.Items.OfType<GliderSkinUnlocker>(),
                "jade_bot_skin_unlocker" => context.Items.OfType<JadeBotSkinUnlocker>(),
                "miniature_unlocker" => context.Items.OfType<MiniatureUnlocker>(),
                "mist_champion_skin_unlocker" => context.Items.OfType<MistChampionSkinUnlocker>(),
                "mount_skin_unlocker" => context.Items.OfType<MountSkinUnlocker>(),
                "outfit_unlocker" => context.Items.OfType<OutfitUnlocker>(),
                "recipe_sheet" => context.Items.OfType<RecipeSheet>(),
                "shared_inventory_slot" => context.Items.OfType<SharedInventorySlot>(),
                "storage_expander" => context.Items.OfType<StorageExpander>(),
                "upgrade_extractor" => context.Items.OfType<UpgradeExtractor>(),
                "utility" => context.Items.OfType<Utility>(),
                "container" => context.Items.OfType<Container>(),
                "default_container" => context.Items.Where(item => EF.Property<string>(item, "Type") == "container"),
                "black_lion_chest" => context.Items.OfType<BlackLionChest>(),
                "gift_box" => context.Items.OfType<GiftBox>(),
                "immediate_container" => context.Items.OfType<ImmediateContainer>(),
                "crafting_material" => context.Items.OfType<CraftingMaterial>(),
                "gathering_tool" => context.Items.OfType<GatheringTool>(),
                "bait" => context.Items.OfType<Bait>(),
                "harvesting_sickle" => context.Items.OfType<HarvestingSickle>(),
                "logging_axe" => context.Items.OfType<LoggingAxe>(),
                "lure" => context.Items.OfType<Lure>(),
                "mining_pick" => context.Items.OfType<MiningPick>(),
                "gizmo" => context.Items.OfType<Gizmo>(),
                "jade_tech_module" => context.Items.OfType<JadeTechModule>(),
                "miniature" => context.Items.OfType<Miniature>(),
                "power_core" => context.Items.OfType<PowerCore>(),
                "relic" => context.Items.OfType<Relic>(),
                "salvage_tool" => context.Items.OfType<SalvageTool>(),
                "trophy" => context.Items.OfType<Trophy>(),
                "upgrade_component" => context.Items.OfType<UpgradeComponent>(),
                "universal_upgrade" => context.Items.OfType<Gem>(),
                "rune" => context.Items.FromSqlRaw(
                    """
                    SELECT *
                    FROM Items item
                    WHERE item.Type = 'rune'
                    AND NOT EXISTS (
                    	SELECT 1
                    	FROM json_each(GameTypes)
                    	WHERE json_each.value = 'Pvp'
                    )
                    """),
                "rune_pvp" => context.Items.FromSqlRaw(
                    """
                    SELECT *
                    FROM Items item
                    WHERE item.Type = 'rune'
                    AND EXISTS (
                    	SELECT 1
                    	FROM json_each(GameTypes)
                    	WHERE json_each.value = 'Pvp'
                    )
                    """),
                "sigil" => context.Items.FromSqlRaw(
                    """
                    SELECT *
                    FROM Items item
                    WHERE item.Type = 'sigil'
                    AND NOT EXISTS (
                    	SELECT 1
                    	FROM json_each(GameTypes)
                    	WHERE json_each.value = 'Pvp'
                    )
                    """),
                "sigil_pvp" => context.Items.FromSqlRaw(
                    """
                    SELECT *
                    FROM Items item
                    WHERE item.Type = 'sigil'
                    AND EXISTS (
                    	SELECT 1
                    	FROM json_each(GameTypes)
                    	WHERE json_each.value = 'Pvp'
                    )
                    """),
                "infusion" => context.Items.FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type = 'upgrade_component'
                    AND InfusionUpgradeFlags -> '$.infusion' = 'true'
                    """),
                "enrichment" => context.Items.FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type = 'upgrade_component'
                    AND InfusionUpgradeFlags -> '$.enrichment' = 'true'
                    """),
                "jewel" => context.Items.FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type = 'upgrade_component'
                    AND UpgradeComponentFlags -> '$.Trinket' = 'true'
                    AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """),
                "glyph" => context.Items.FromSqlRaw(
                    """
                    SELECT *
                    FROM Items
                    WHERE Type = 'upgrade_component'
                    AND UpgradeComponentFlags -> '$.Axe' = 'false'
                    AND UpgradeComponentFlags -> '$.Trinket' = 'false'
                    AND UpgradeComponentFlags -> '$.MediumArmor' = 'false'
                    AND InfusionUpgradeFlags -> '$.infusion' = 'false'
                    AND InfusionUpgradeFlags -> '$.enrichment' = 'false'
                    """),
                "expansions" => context.Items
                    .Where(item => item is BagSlotExpansion
                        || item is BankTabExpansion
                        || item is StorageExpander
                        || item is BuildStorageExpansion
                        || item is BuildTemplateExpansion
                        || item is EquipmentTemplateExpansion
                        || item is SharedInventorySlot
                    ),
                _ => context.Items
            };

            if (!string.IsNullOrWhiteSpace(filter.Text))
            {
                query = query.Where(item => EF.Functions.Like(item.Name, $"%{filter.Text}%"))
                    .OrderBy(item => Levenshtein.LevenshteinDistance(filter.Text!, item.Name));
            }
            else
            {
                query = query
                    .OrderByDescending(item => item.Id);
            }

            resultContext.ResultTotal = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            await foreach (Item? item in query
               .Take(limit)
               .AsAsyncEnumerable()
               .WithCancellation(cancellationToken)
               .ConfigureAwait(false))
            {
                yield return item;
            }
        }
    }
}
