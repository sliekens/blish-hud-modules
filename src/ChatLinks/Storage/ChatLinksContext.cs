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
        ThrowHelper.ThrowIfNull(optionsBuilder);
        if (!optionsBuilder.IsConfigured)
        {
            _ = optionsBuilder.UseSqlite("Data Source=data.db");
        }

        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
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
        ThrowHelper.ThrowIfNull(modelBuilder);
        _ = modelBuilder.ApplyConfiguration(new ItemEntityTypeConfiguration());

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

        _ = modelBuilder.ApplyConfiguration(new ArmorEntityTypeConfiguration(
            attributesConverter,
            attributesComparer,
            buffComparer,
            infusionSlotsComparer
        ));

        _ = modelBuilder.ApplyConfiguration(new TrinketEntityTypeConfiguration(
            attributesConverter,
            attributesComparer,
            buffComparer,
            infusionSlotsComparer
        ));

        _ = modelBuilder.ApplyConfiguration(new RingEntityTypeConfiguration(
            upgradeSourceComparer,
            upgradePathComparer
        ));

        _ = modelBuilder.ApplyConfiguration(new WeaponEntityTypeConfiguration(
            attributesConverter,
            attributesComparer,
            buffComparer,
            infusionSlotsComparer
        ));

        _ = modelBuilder.ApplyConfiguration(new WeaponEntityTypeConfiguration(
            attributesConverter,
            attributesComparer,
            buffComparer,
            infusionSlotsComparer
        ));

        _ = modelBuilder.ApplyConfiguration(new BackItemEntityTypeConfiguration(
            attributesConverter,
            attributesComparer,
            buffComparer,
            infusionSlotsComparer,
            upgradeSourceComparer,
            upgradePathComparer
        ));

        _ = modelBuilder.ApplyConfiguration(new UpgradeComponentEntityTypeConfiguration(
            attributesConverter,
            attributesComparer,
            buffComparer
        ));

        _ = modelBuilder.ApplyConfiguration(new RuneEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new CraftingMaterialEntityTypeConfiguration(upgradePathComparer));

        _ = modelBuilder.ApplyConfiguration(new RecipeSheetEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new TransmutationEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new FoodEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new GenericConsumableEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new ServiceEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new GizmoEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new UtilityEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new EquipmentSkinEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new ArmorSkinEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new WeaponSkinEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new DyeColorEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new RecipeEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new GuildConsumableRecipeEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new GuildDecorationRecipeEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new GuildWvwUpgradeRecipeEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new FinisherEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new GliderSkinEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new JadeBotSkinEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new MailCarrierEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new MiniatureEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new MistChampionSkinEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new NoveltyEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new OutfitEntityTypeConfiguration());
    }
}
