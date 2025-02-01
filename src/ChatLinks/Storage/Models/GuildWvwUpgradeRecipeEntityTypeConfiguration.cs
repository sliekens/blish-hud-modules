using GuildWars2.Hero.Crafting.Recipes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SL.ChatLinks.Storage.Models;

public sealed class GuildWvwUpgradeRecipeEntityTypeConfiguration : IEntityTypeConfiguration<GuildWvwUpgradeRecipe>
{
    public void Configure(EntityTypeBuilder<GuildWvwUpgradeRecipe> builder)
    {
        builder.Property(recipe => recipe.OutputUpgradeId)
            .HasColumnName("OutputWvwUpgradeId");
    }
}