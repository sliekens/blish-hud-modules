﻿using System.Text.Json;

using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;

using SL.ChatLinks.Logging;

namespace SL.ChatLinks.Storage;

public class ChatLinksContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Item> Items => Set<Item>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=data.db");
        }

        optionsBuilder.UseLoggerFactory(LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new LoggingAdapterProvider<ChatLinksContext>());
        }));
    }

    private static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value);
    }

    private static T? Deserialize<T>(string value)
    {
        return JsonSerializer.Deserialize<T>(value);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        EntityTypeBuilder<Item> itemBuilder = modelBuilder.Entity<Item>();
        itemBuilder.HasDiscriminator<string>("Type")
            .HasValue<Item>("item")
            .HasValue<CraftingMaterial>("crafting_material")
            .HasValue<JadeTechModule>("jade_tech_module")
            .HasValue<PowerCore>("power_core")
            .HasValue<Bag>("bag")
            .HasValue<Relic>("relic")
            .HasValue<Miniature>("miniature")
            .HasValue<Trophy>("trophy")
            .HasValue<Backpack>("back")
            .HasValue<SalvageTool>("salvage_tool")
            .HasValue<Container>("container")
            .HasValue<ImmediateContainer>("immediate_container")
            .HasValue<BlackLionChest>("black_lion_chest")
            .HasValue<GiftBox>("gift_box")
            .HasValue<UpgradeComponent>("upgrade_component")
            .HasValue<Sigil>("sigil")
            .HasValue<Gem>("gem")
            .HasValue<Rune>("rune")
            .HasValue<Trinket>("trinket")
            .HasValue<Ring>("ring")
            .HasValue<Amulet>("amulet")
            .HasValue<Accessory>("accessory")
            .HasValue<Armor>("armor")
            .HasValue<Helm>("helm")
            .HasValue<HelmAquatic>("helm_aquatic")
            .HasValue<Shoulders>("shoulders")
            .HasValue<Coat>("coat")
            .HasValue<Gloves>("gloves")
            .HasValue<Leggings>("leggings")
            .HasValue<Boots>("boots")
            .HasValue<Weapon>("weapon")
            .HasValue<Greatsword>("greatsword")
            .HasValue<Torch>("torch")
            .HasValue<Staff>("staff")
            .HasValue<HarpoonGun>("harpoon_gun")
            .HasValue<Longbow>("longbow")
            .HasValue<Shortbow>("shortbow")
            .HasValue<SmallBundle>("small_bundle")
            .HasValue<LargeBundle>("large_bundle")
            .HasValue<Pistol>("pistol")
            .HasValue<Mace>("mace")
            .HasValue<Rifle>("rifle")
            .HasValue<Spear>("spear")
            .HasValue<Dagger>("dagger")
            .HasValue<Scepter>("scepter")
            .HasValue<Sword>("sword")
            .HasValue<Trident>("trident")
            .HasValue<Hammer>("hammer")
            .HasValue<Warhorn>("warhorn")
            .HasValue<Focus>("focus")
            .HasValue<Toy>("toy")
            .HasValue<ToyTwoHanded>("toy_two_handed")
            .HasValue<Axe>("axe")
            .HasValue<Shield>("shield")
            .HasValue<GatheringTool>("gathering_tool")
            .HasValue<Lure>("lure")
            .HasValue<Bait>("bait")
            .HasValue<MiningPick>("mining_pick")
            .HasValue<HarvestingSickle>("harvesting_sickle")
            .HasValue<LoggingAxe>("logging_axe")
            .HasValue<Gizmo>("gizmo")
            .HasValue<BlackLionChestKey>("black_lion_chest_key")
            .HasValue<UnlimitedConsumable>("unlimited_consumable")
            .HasValue<RentableContractNpc>("rentable_contract_npc")
            .HasValue<Consumable>("consumable")
            .HasValue<Food>("food")
            .HasValue<Utility>("utility")
            .HasValue<Transmutation>("transmutation")
            .HasValue<GenericConsumable>("generic_consumable")
            .HasValue<Currency>("currency")
            .HasValue<Booze>("booze")
            .HasValue<ContractNpc>("contract_npc")
            .HasValue<MountLicense>("mount_license")
            .HasValue<UpgradeExtractor>("upgrade_extractor")
            .HasValue<Service>("service")
            .HasValue<RandomUnlocker>("random_unlocker")
            .HasValue<HalloweenConsumable>("halloween_consumable")
            .HasValue<TeleportToFriend>("teleport_to_friend")
            .HasValue<AppearanceChanger>("appearance_changer")
            .HasValue<Unlocker>("unlocker")
            .HasValue<RecipeSheet>("recipe_sheet")
            .HasValue<BankTabExpansion>("bank_tab_expansion")
            .HasValue<BagSlotExpansion>("bag_slot_expansion")
            .HasValue<EquipmentTemplateExpansion>("equipment_template_expansion")
            .HasValue<BuildTemplateExpansion>("build_template_expansion")
            .HasValue<BuildStorageExpansion>("build_storage_expansion")
            .HasValue<Dye>("dye")
            .HasValue<StorageExpander>("storage_expander")
            .HasValue<ContentUnlocker>("content_unlocker")
            .HasValue<OutfitUnlocker>("outfit_unlocker")
            .HasValue<GliderSkinUnlocker>("glider_skin_unlocker")
            .HasValue<MountSkinUnlocker>("mount_skin_unlocker")
            .HasValue<MiniatureUnlocker>("miniature_unlocker")
            .HasValue<SharedInventorySlot>("shared_inventory_slot")
            .HasValue<MistChampionSkinUnlocker>("mist_champion_skin_unlocker")
            .HasValue<JadeBotSkinUnlocker>("jade_bot_skin_unlocker")
            ;

        itemBuilder.Property(item => item.Id).ValueGeneratedNever();
        itemBuilder.Property(item => item.Rarity).HasConversion(new ExtensibleEnumConverter<Rarity>());
        itemBuilder.Property(item => item.VendorValue).HasConversion(new CoinConverter());
        itemBuilder.Property(item => item.GameTypes).HasJsonValueConversion();
        itemBuilder.Property(item => item.Flags).HasJsonValueConversion();
        itemBuilder.Property(item => item.Restrictions).HasJsonValueConversion();

        ValueConverter<IDictionary<Extensible<AttributeName>, int>, string> attributesConverter = new(
            static attr => Serialize(attr.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value)),
            static json => Deserialize<Dictionary<string, int>>(json)
                .ToDictionary(pair => new Extensible<AttributeName>(pair.Key), pair => pair.Value));

        EntityTypeBuilder<Armor> armorBuilder = modelBuilder.Entity<Armor>();
        armorBuilder.Property(armor => armor.WeightClass).HasConversion(new ExtensibleEnumConverter<WeightClass>());
        armorBuilder.Property(armor => armor.Defense).HasColumnName("Defense");
        armorBuilder.Property(armor => armor.DefaultSkinId).HasColumnName("DefaultSkinId");
        armorBuilder.Property(armor => armor.SuffixItemId).HasColumnName("SuffixItemId");
        armorBuilder.Property(armor => armor.AttributeCombinationId).HasColumnName("AttributeCombinationId");
        armorBuilder.Property(armor => armor.Attributes).HasColumnName("Attributes").HasConversion(attributesConverter);
        armorBuilder.Property(armor => armor.AttributeAdjustment).HasColumnName("AttributeAdjustment");
        armorBuilder.Property(armor => armor.StatChoices).HasColumnName("StatChoices").HasJsonValueConversion();
        armorBuilder.Property(armor => armor.InfusionSlots).HasColumnName("InfusionSlots").HasJsonValueConversion();
        armorBuilder.Property(armor => armor.Buff).HasColumnName("Buff").HasJsonValueConversion();

        EntityTypeBuilder<Trinket> trinketBuilder = modelBuilder.Entity<Trinket>();
        trinketBuilder.Property(trinket => trinket.SuffixItemId).HasColumnName("SuffixItemId");
        trinketBuilder.Property(trinket => trinket.AttributeCombinationId).HasColumnName("AttributeCombinationId");
        trinketBuilder.Property(trinket => trinket.Attributes).HasColumnName("Attributes")
            .HasConversion(attributesConverter);
        trinketBuilder.Property(trinket => trinket.AttributeAdjustment).HasColumnName("AttributeAdjustment");
        trinketBuilder.Property(trinket => trinket.StatChoices).HasColumnName("StatChoices").HasJsonValueConversion();
        trinketBuilder.Property(trinket => trinket.InfusionSlots).HasColumnName("InfusionSlots")
            .HasJsonValueConversion();
        trinketBuilder.Property(trinket => trinket.Buff).HasColumnName("Buff").HasJsonValueConversion();

        EntityTypeBuilder<Ring> ringBuilder = modelBuilder.Entity<Ring>();
        ringBuilder.Property(ring => ring.UpgradesFrom).HasColumnName("UpgradesFrom").HasJsonValueConversion();
        ringBuilder.Property(ring => ring.UpgradesInto).HasColumnName("UpgradesInto").HasJsonValueConversion();

        EntityTypeBuilder<Weapon> weaponBuilder = modelBuilder.Entity<Weapon>();
        weaponBuilder.Property(weapon => weapon.DamageType).HasConversion(new ExtensibleEnumConverter<DamageType>());
        weaponBuilder.Property(weapon => weapon.Defense).HasColumnName("Defense");
        weaponBuilder.Property(weapon => weapon.DefaultSkinId).HasColumnName("DefaultSkinId");
        weaponBuilder.Property(weapon => weapon.SuffixItemId).HasColumnName("SuffixItemId");
        weaponBuilder.Property(weapon => weapon.SecondarySuffixItemId).HasColumnName("SecondarySuffixItemId");
        weaponBuilder.Property(weapon => weapon.AttributeCombinationId).HasColumnName("AttributeCombinationId");
        weaponBuilder.Property(weapon => weapon.Attributes).HasColumnName("Attributes").HasConversion(attributesConverter);
        weaponBuilder.Property(weapon => weapon.AttributeAdjustment).HasColumnName("AttributeAdjustment");
        weaponBuilder.Property(weapon => weapon.StatChoices).HasColumnName("StatChoices").HasJsonValueConversion();
        weaponBuilder.Property(weapon => weapon.InfusionSlots).HasColumnName("InfusionSlots").HasJsonValueConversion();
        weaponBuilder.Property(weapon => weapon.Buff).HasColumnName("Buff").HasJsonValueConversion();

        EntityTypeBuilder<Backpack> backpackBuilder = modelBuilder.Entity<Backpack>();
        backpackBuilder.Property(backpack => backpack.DefaultSkinId).HasColumnName("DefaultSkinId");
        backpackBuilder.Property(backpack => backpack.SuffixItemId).HasColumnName("SuffixItemId");
        backpackBuilder.Property(backpack => backpack.AttributeCombinationId).HasColumnName("AttributeCombinationId");
        backpackBuilder.Property(backpack => backpack.Attributes).HasColumnName("Attributes")
            .HasConversion(attributesConverter);
        backpackBuilder.Property(backpack => backpack.AttributeAdjustment).HasColumnName("AttributeAdjustment");
        backpackBuilder.Property(backpack => backpack.StatChoices).HasColumnName("StatChoices")
            .HasJsonValueConversion();
        backpackBuilder.Property(backpack => backpack.InfusionSlots).HasColumnName("InfusionSlots")
            .HasJsonValueConversion();
        backpackBuilder.Property(backpack => backpack.Buff).HasColumnName("Buff").HasJsonValueConversion();
        backpackBuilder.Property(backpack => backpack.UpgradesFrom).HasColumnName("UpgradesFrom")
            .HasJsonValueConversion();
        backpackBuilder.Property(backpack => backpack.UpgradesInto).HasColumnName("UpgradesInto")
            .HasJsonValueConversion();

        EntityTypeBuilder<UpgradeComponent> upgradeComponentBuilder = modelBuilder.Entity<UpgradeComponent>();
        upgradeComponentBuilder.Property(upgradeComponent => upgradeComponent.AttributeCombinationId)
            .HasColumnName("AttributeCombinationId");
        upgradeComponentBuilder.Property(upgradeComponent => upgradeComponent.Attributes)
            .HasColumnName("Attributes")
            .HasConversion(attributesConverter);
        upgradeComponentBuilder.Property(upgradeComponent => upgradeComponent.AttributeAdjustment)
            .HasColumnName("AttributeAdjustment");
        upgradeComponentBuilder.Property(upgradeComponent => upgradeComponent.UpgradeComponentFlags)
            .HasConversion(new JsonValueConverter<UpgradeComponentFlags>());
        upgradeComponentBuilder.Property(upgradeComponent => upgradeComponent.InfusionUpgradeFlags)
            .HasConversion(new JsonValueConverter<InfusionSlotFlags>());
        upgradeComponentBuilder.Property(upgradeComponent => upgradeComponent.Buff).HasColumnName("Buff")
            .HasJsonValueConversion();

        EntityTypeBuilder<Rune> runeBuilder = modelBuilder.Entity<Rune>();
        runeBuilder.Property(rune => rune.Bonuses).HasJsonValueConversion();

        EntityTypeBuilder<CraftingMaterial> craftingMaterialBuilder = modelBuilder.Entity<CraftingMaterial>();
        craftingMaterialBuilder.Property(craftingMaterial => craftingMaterial.UpgradesInto)
            .HasColumnName("UpgradesInto").HasJsonValueConversion();

        EntityTypeBuilder<RecipeSheet> recipeSheetBuilder = modelBuilder.Entity<RecipeSheet>();
        recipeSheetBuilder.Property(recipeSheet => recipeSheet.ExtraRecipeIds).HasJsonValueConversion();

        EntityTypeBuilder<Transmutation> transmutationBuilder = modelBuilder.Entity<Transmutation>();
        transmutationBuilder.Property(transmutation => transmutation.SkinIds).HasJsonValueConversion();

        EntityTypeBuilder<Food> foodBuilder = modelBuilder.Entity<Food>();
        foodBuilder.Property(food => food.Effect).HasColumnName("Effect").HasJsonValueConversion();

        EntityTypeBuilder<GenericConsumable> genericConsumableBuilder = modelBuilder.Entity<GenericConsumable>();
        genericConsumableBuilder.Property(genericConsumable => genericConsumable.Effect).HasColumnName("Effect")
            .HasJsonValueConversion();
        genericConsumableBuilder.Property(genericConsumable => genericConsumable.GuildUpgradeId)
            .HasColumnName("GuildUpgradeId");

        EntityTypeBuilder<Service> serviceBuilder = modelBuilder.Entity<Service>();
        serviceBuilder.Property(service => service.Effect).HasColumnName("Effect").HasJsonValueConversion();
        serviceBuilder.Property(service => service.GuildUpgradeId).HasColumnName("GuildUpgradeId");

        var gizmoBuilder = modelBuilder.Entity<Gizmo>();
        gizmoBuilder.Property(gizmo => gizmo.GuildUpgradeId).HasColumnName("GuildUpgradeId");

        EntityTypeBuilder<Utility> utilityBuilder = modelBuilder.Entity<Utility>();
        utilityBuilder.Property(utility => utility.Effect).HasColumnName("Effect").HasJsonValueConversion();
    }
}