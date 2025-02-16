using System.Text.Json;

using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Hero.Crafting.Recipes;
using GuildWars2.Hero.Equipment.Dyes;
using GuildWars2.Hero.Equipment.Finishers;
using GuildWars2.Hero.Equipment.Gliders;
using GuildWars2.Hero.Equipment.JadeBots;
using GuildWars2.Hero.Equipment.MailCarriers;
using GuildWars2.Hero.Equipment.Novelties;
using GuildWars2.Hero.Equipment.Outfits;
using GuildWars2.Hero.Equipment.Wardrobe;
using GuildWars2.Items;
using GuildWars2.Pvp.MistChampions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Models.Hero.Crafting;
using SL.ChatLinks.Storage.Models.Hero.Equipment.Dyes;
using SL.ChatLinks.Storage.Models.Hero.Equipment.Finishers;
using SL.ChatLinks.Storage.Models.Hero.Equipment.Gliders;
using SL.ChatLinks.Storage.Models.Hero.Equipment.JadeBots;
using SL.ChatLinks.Storage.Models.Hero.Equipment.MailCarriers;
using SL.ChatLinks.Storage.Models.Hero.Equipment.Miniatures;
using SL.ChatLinks.Storage.Models.Hero.Equipment.Novelties;
using SL.ChatLinks.Storage.Models.Hero.Equipment.Outfits;
using SL.ChatLinks.Storage.Models.Hero.Equipment.Wardrobe;
using SL.ChatLinks.Storage.Models.Items;
using SL.ChatLinks.Storage.Models.Pvp.MistChampions;

namespace SL.ChatLinks.Storage;

public class ChatLinksContext(DbContextOptions options) : DbContext(options)
{
    public static int SchemaVersion => 3;

    public DbSet<Item> Items => Set<Item>();

    public DbSet<EquipmentSkin> Skins => Set<EquipmentSkin>();

    public DbSet<Recipe> Recipes => Set<Recipe>();

    public DbSet<DyeColor> Colors => Set<DyeColor>();

    public DbSet<Finisher> Finishers => Set<Finisher>();

    public DbSet<GliderSkin> Gliders => Set<GliderSkin>();

    public DbSet<JadeBotSkin> JadeBots => Set<JadeBotSkin>();

    public DbSet<MailCarrier> MailCarrriers => Set<MailCarrier>();

    public DbSet<MistChampionSkin> MistChampions => Set<MistChampionSkin>();

    public DbSet<Novelty> Novelties => Set<Novelty>();

    public DbSet<GuildWars2.Hero.Equipment.Miniatures.Miniature> Miniatures => Set<GuildWars2.Hero.Equipment.Miniatures.Miniature>();

    public DbSet<Outfit> Outfits => Set<Outfit>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=data.db");
        }
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
        modelBuilder.ApplyConfiguration(new ItemEntityTypeConfiguration());

        ValueConverter<IDictionary<Extensible<AttributeName>, int>, string> attributesConverter = new(
            static attr => Serialize(attr.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value)),
            static json => Deserialize<Dictionary<string, int>>(json)
                .ToDictionary(pair => new Extensible<AttributeName>(pair.Key), pair => pair.Value));

        ValueComparer<IDictionary<Extensible<AttributeName>, int>> attributesComparer =
            new DictionaryComparer<Extensible<AttributeName>, int>();

        ValueComparer<Buff> buffComparer = new ValueObjectComparer<Buff>(buff => new Buff
        {
            Description = buff.Description,
            SkillId = buff.SkillId
        });

        ValueComparer<IReadOnlyList<InfusionSlot>> infusionSlotsComparer = new ListComparer<InfusionSlot>();

        ValueComparer<IReadOnlyCollection<InfusionSlotUpgradeSource>> upgradeSourceComparer =
            new CollectionComparer<InfusionSlotUpgradeSource>();

        ValueComparer<IReadOnlyCollection<InfusionSlotUpgradePath>> upgradePathComparer =
            new CollectionComparer<InfusionSlotUpgradePath>();

        modelBuilder.ApplyConfiguration(new ArmorEntityTypeConfiguration(
            attributesConverter,
            attributesComparer,
            buffComparer,
            infusionSlotsComparer
        ));

        modelBuilder.ApplyConfiguration(new TrinketEntityTypeConfiguration(
            attributesConverter,
            attributesComparer,
            buffComparer,
            infusionSlotsComparer
        ));

        modelBuilder.ApplyConfiguration(new RingEntityTypeConfiguration(
            upgradeSourceComparer,
            upgradePathComparer
        ));

        modelBuilder.ApplyConfiguration(new WeaponEntityTypeConfiguration(
            attributesConverter,
            attributesComparer,
            buffComparer,
            infusionSlotsComparer
        ));

        modelBuilder.ApplyConfiguration(new WeaponEntityTypeConfiguration(
            attributesConverter,
            attributesComparer,
            buffComparer,
            infusionSlotsComparer
        ));

        modelBuilder.ApplyConfiguration(new BackItemEntityTypeConfiguration(
            attributesConverter,
            attributesComparer,
            buffComparer,
            infusionSlotsComparer,
            upgradeSourceComparer,
            upgradePathComparer
        ));

        modelBuilder.ApplyConfiguration(new UpgradeComponentEntityTypeConfiguration(
            attributesConverter,
            attributesComparer,
            buffComparer
        ));

        modelBuilder.ApplyConfiguration(new RuneEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new CraftingMaterialEntityTypeConfiguration(upgradePathComparer));

        modelBuilder.ApplyConfiguration(new RecipeSheetEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new TransmutationEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new FoodEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new GenericConsumableEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new ServiceEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new GizmoEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new UtilityEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new EquipmentSkinEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new ArmorSkinEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new WeaponSkinEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new DyeColorEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new RecipeEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new GuildConsumableRecipeEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new GuildDecorationRecipeEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new GuildWvwUpgradeRecipeEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new FinisherEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new GliderSkinEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new JadeBotSkinEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new MailCarrierEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new MiniatureEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new MistChampionSkinEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new NoveltyEntityTypeConfiguration());

        modelBuilder.ApplyConfiguration(new OutfitEntityTypeConfiguration());
    }
}