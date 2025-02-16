using GuildWars2;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Items;

public sealed class ItemEntityTypeConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasIndex(item => item.Name);
        builder.HasIndex(item => item.ChatLink);
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.Rarity).HasConversion(new ExtensibleEnumConverter<Rarity>());
        builder.Property(item => item.VendorValue).HasConversion(new CoinConverter());
        builder.Property(item => item.GameTypes)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new CollectionComparer<Extensible<GameType>>());
        builder.Property(item => item.Flags).HasJsonValueConversion();
        builder.Property(item => item.Restrictions).HasJsonValueConversion();

        builder.HasDiscriminator<string>("Type")
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
    }
}
