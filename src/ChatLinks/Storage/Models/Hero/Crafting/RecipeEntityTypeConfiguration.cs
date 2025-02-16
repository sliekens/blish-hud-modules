using GuildWars2;
using GuildWars2.Hero.Crafting;
using GuildWars2.Hero.Crafting.Disciplines;
using GuildWars2.Hero.Crafting.Recipes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Hero.Crafting;

public sealed class RecipeEntityTypeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("Recipes");
        builder.HasKey(recipe => recipe.Id);
        builder.HasIndex(recipe => recipe.ChatLink);
        builder.HasIndex(recipe => recipe.OutputItemId);
        builder.Property(recipe => recipe.Disciplines).HasJsonValueConversion()
            .Metadata.SetValueComparer(new CollectionComparer<Extensible<CraftingDisciplineName>>());
        builder.Property(recipe => recipe.Flags).HasJsonValueConversion();
        builder.Property(recipe => recipe.Ingredients).HasJsonValueConversion()
            .Metadata.SetValueComparer(new ListComparer<Ingredient>());

        var discriminatorBuilder = builder.HasDiscriminator<string>("Type");
        discriminatorBuilder.HasValue<Recipe>("recipe")
            .HasValue<AmuletRecipe>("amulet")
            .HasValue<AxeRecipe>("axe")
            .HasValue<BackpackRecipe>("backpack")
            .HasValue<BagRecipe>("bag")
            .HasValue<BootsRecipe>("boots")
            .HasValue<BulkRecipe>("bulk")
            .HasValue<CoatRecipe>("coat")
            .HasValue<ComponentRecipe>("component")
            .HasValue<ConsumableRecipe>("consumable")
            .HasValue<DaggerRecipe>("dagger")
            .HasValue<DessertRecipe>("dessert")
            .HasValue<DyeRecipe>("dye")
            .HasValue<EarringRecipe>("earring")
            .HasValue<FeastRecipe>("feast")
            .HasValue<FocusRecipe>("focus")
            .HasValue<GlovesRecipe>("gloves")
            .HasValue<GreatswordRecipe>("greatsword")
            .HasValue<GuildConsumableRecipe>("guild_consumable")
            .HasValue<GuildDecorationRecipe>("guild_decoration")
            .HasValue<GuildWvwUpgradeRecipe>("guild_wvw_upgrade")
            .HasValue<HammerRecipe>("hammer")
            .HasValue<HarpoonGunRecipe>("harpoon_gun")
            .HasValue<HeadgearRecipe>("headgear")
            .HasValue<IngredientCookingRecipe>("ingredient_cooking")
            .HasValue<InscriptionRecipe>("inscription")
            .HasValue<InsigniaRecipe>("insignia")
            .HasValue<LegendaryComponentRecipe>("legendary_component")
            .HasValue<LeggingsRecipe>("leggings")
            .HasValue<LongbowRecipe>("longbow")
            .HasValue<MaceRecipe>("mace")
            .HasValue<MealRecipe>("meal")
            .HasValue<PistolRecipe>("pistol")
            .HasValue<PotionRecipe>("potion")
            .HasValue<RefinementEctoplasmRecipe>("refinement_ectoplasm")
            .HasValue<RefinementObsidianRecipe>("refinement_obsidian")
            .HasValue<RefinementRecipe>("refinement")
            .HasValue<RifleRecipe>("rifle")
            .HasValue<RingRecipe>("ring")
            .HasValue<ScepterRecipe>("scepter")
            .HasValue<SeasoningRecipe>("seasoning")
            .HasValue<ShieldRecipe>("shield")
            .HasValue<ShortbowRecipe>("shortbow")
            .HasValue<ShouldersRecipe>("shoulders")
            .HasValue<SnackRecipe>("snack")
            .HasValue<SoupRecipe>("soup")
            .HasValue<SpearRecipe>("spear")
            .HasValue<StaffRecipe>("staff")
            .HasValue<SwordRecipe>("sword")
            .HasValue<TorchRecipe>("torch")
            .HasValue<TridentRecipe>("trident")
            .HasValue<UpgradeComponentRecipe>("upgrade_component")
            .HasValue<WarhornRecipe>("warhorn");
        ;
    }
}
