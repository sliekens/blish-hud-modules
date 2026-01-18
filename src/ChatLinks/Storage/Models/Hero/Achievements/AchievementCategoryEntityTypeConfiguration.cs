using GuildWars2.Hero.Achievements.Categories;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Hero.Achievements;

public sealed class AchievementCategoryEntityTypeConfiguration : IEntityTypeConfiguration<AchievementCategory>
{
    public void Configure(EntityTypeBuilder<AchievementCategory> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.Property(achievementCategory => achievementCategory.Id).ValueGeneratedNever();

        _ = builder.Property(achievementCategory => achievementCategory.Achievements)
            .HasJsonValueConversion();

        _ = builder.Property(achievementCategory => achievementCategory.Tomorrow)
            .HasJsonValueConversion();

        _ = builder.HasIndex(achievementCategory => achievementCategory.Name);

        _ = builder.HasIndex(achievementCategory => achievementCategory.Order);
    }
}
