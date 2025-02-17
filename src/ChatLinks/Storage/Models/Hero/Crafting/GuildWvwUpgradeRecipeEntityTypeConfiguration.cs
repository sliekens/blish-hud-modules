using GuildWars2.Hero.Crafting.Recipes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SL.ChatLinks.Storage.Models.Hero.Crafting;

public sealed class GuildWvwUpgradeRecipeEntityTypeConfiguration : IEntityTypeConfiguration<GuildWvwUpgradeRecipe>
{
    public void Configure(EntityTypeBuilder<GuildWvwUpgradeRecipe> builder)
    {
        _ = builder.Property(recipe => recipe.OutputUpgradeId)
            .HasColumnName("OutputWvwUpgradeId");
    }
}
