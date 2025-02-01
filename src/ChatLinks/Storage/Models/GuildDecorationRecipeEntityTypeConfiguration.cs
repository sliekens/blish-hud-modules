using GuildWars2.Hero.Crafting;
using GuildWars2.Hero.Crafting.Recipes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models;

public sealed class GuildDecorationRecipeEntityTypeConfiguration : IEntityTypeConfiguration<GuildDecorationRecipe>
{
    public void Configure(EntityTypeBuilder<GuildDecorationRecipe> builder)
    {
        builder.Property(recipe => recipe.GuildIngredients)
            .HasColumnName(nameof(GuildDecorationRecipe.GuildIngredients))
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new ListComparer<GuildIngredient>());

        builder.Property(recipe => recipe.OutputUpgradeId)
            .HasColumnName(nameof(GuildDecorationRecipe.OutputUpgradeId));
    }
}