using GuildWars2.Hero.Achievements;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Hero.Achievements;

public sealed class AchievementEntityTypeConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.Property(achievement => achievement.Id)
            .ValueGeneratedNever();

        _ = builder.Property(achievement => achievement.Flags)
            .HasJsonValueConversion();

        _ = builder.Property(achievement => achievement.Tiers)
             .HasJsonValueConversion();

        _ = builder.Property(achievement => achievement.Rewards)
            .HasJsonValueConversion();

        _ = builder.Property(achievement => achievement.Bits)
            .HasJsonValueConversion();

        _ = builder.Property(achievement => achievement.Prerequisites)
            .HasJsonValueConversion();

        _ = builder.HasIndex(achievement => achievement.Name);

        _ = builder.HasDiscriminator<string>("Type")
           .HasValue<Achievement>("achievement")
           .HasValue<CollectionAchievement>("collection_achievement");
    }
}
