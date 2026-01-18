using System.Reflection;
using System.Text.Json;

using GuildWars2.Hero.Achievements;
using GuildWars2.Hero.Achievements.Categories;
using GuildWars2.Hero.Achievements.Groups;
using GuildWars2.Hero.Crafting.Recipes;
using GuildWars2.Hero.Equipment.Dyes;
using GuildWars2.Hero.Equipment.Finishers;
using GuildWars2.Hero.Equipment.Gliders;
using GuildWars2.Hero.Equipment.JadeBots;
using GuildWars2.Hero.Equipment.MailCarriers;
using GuildWars2.Hero.Equipment.Miniatures;
using GuildWars2.Hero.Equipment.Novelties;
using GuildWars2.Hero.Equipment.Outfits;
using GuildWars2.Hero.Equipment.Wardrobe;
using GuildWars2.Items;
using GuildWars2.Pvp.MistChampions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Models.Hero.Achievements;
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
    public static int SchemaVersion => 7;

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

    public DbSet<Miniature> Miniatures => Set<Miniature>();

    public DbSet<Outfit> Outfits => Set<Outfit>();

    public DbSet<Achievement> Achievements => Set<Achievement>();

    public DbSet<AchievementCategory> AchievementCategories => Set<AchievementCategory>();

    public DbSet<AchievementGroup> AchievementGroups => Set<AchievementGroup>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        ThrowHelper.ThrowIfNull(optionsBuilder);
        if (!optionsBuilder.IsConfigured)
        {
            _ = optionsBuilder.UseSqlite("Data Source=data.db");
        }

        _ = optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
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

        _ = modelBuilder.ApplyConfiguration(new ArmorEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new TrinketEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new RingEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new WeaponEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new WeaponEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new BackItemEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new UpgradeComponentEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new RuneEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new CraftingMaterialEntityTypeConfiguration());

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

        _ = modelBuilder.ApplyConfiguration(new AchievementEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new AchievementCategoryEntityTypeConfiguration());

        _ = modelBuilder.ApplyConfiguration(new AchievementGroupEntityTypeConfiguration());

        MethodInfo levenshteinMethod = typeof(Levenshtein).GetMethod(
            nameof(Levenshtein.LevenshteinDistance)
        )!;

        _ = modelBuilder.HasDbFunction(levenshteinMethod, b =>
        {
            _ = b.HasName(nameof(Levenshtein.LevenshteinDistance));
            _ = b.HasParameter("a");
            _ = b.HasParameter("b");
        });
    }
}
