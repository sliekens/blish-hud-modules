using GuildWars2.Hero.Achievements.Categories;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Hero.Achievements;

public sealed class AchievementCategoryEntityTypeConfiguration : IEntityTypeConfiguration<AchievementCategory>
{
    public void Configure(EntityTypeBuilder<AchievementCategory> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.Property(achievementCategory => achievementCategory.Id).ValueGeneratedNever();

        builder.Property(achievementCategory => achievementCategory.Achievements)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new ListComparer<AchievementRef>());

        builder.Property(achievementCategory => achievementCategory.Tomorrow)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new ListComparer<AchievementRef>());

        _ = builder.HasIndex(achievementCategory => achievementCategory.Name);

        _ = builder.HasIndex(achievementCategory => achievementCategory.Order);

    }
}
