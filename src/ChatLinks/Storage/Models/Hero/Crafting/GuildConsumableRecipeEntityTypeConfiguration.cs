using GuildWars2.Hero.Crafting;
using GuildWars2.Hero.Crafting.Recipes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Hero.Crafting;

public sealed class GuildConsumableRecipeEntityTypeConfiguration : IEntityTypeConfiguration<GuildConsumableRecipe>
{
    public void Configure(EntityTypeBuilder<GuildConsumableRecipe> builder)
    {
        builder.Property(recipe => recipe.GuildIngredients)
            .HasColumnName(nameof(GuildConsumableRecipe.GuildIngredients))
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new ListComparer<GuildIngredient>());

        builder.Property(recipe => recipe.OutputUpgradeId)
            .HasColumnName(nameof(GuildConsumableRecipe.OutputUpgradeId));
    }
}