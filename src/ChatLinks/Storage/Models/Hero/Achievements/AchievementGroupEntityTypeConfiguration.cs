using GuildWars2.Hero.Achievements.Groups;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Hero.Achievements;

public sealed class AchievementGroupEntityTypeConfiguration : IEntityTypeConfiguration<AchievementGroup>
{
    public void Configure(EntityTypeBuilder<AchievementGroup> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.Property(achievementGroup => achievementGroup.Id)
            .ValueGeneratedNever();

        builder.Property(achievementGroup => achievementGroup.Categories)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new ListComparer<int>());

        _ = builder.HasIndex(achievementGroup => achievementGroup.Name);

        _ = builder.HasIndex(achievementGroup => achievementGroup.Order);

    }
}
