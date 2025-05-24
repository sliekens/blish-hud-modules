using GuildWars2.Hero.Achievements;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Comparers;
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

        builder.Property(achievement => achievement.Tiers)
             .HasJsonValueConversion()
             .Metadata.SetValueComparer(new ListComparer<AchievementTier>());

        builder.Property(achievement => achievement.Rewards)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new ListComparer<AchievementReward>());

        builder.Property(achievement => achievement.Bits)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new ListComparer<AchievementBit>());

        builder.Property(achievement => achievement.Prerequisites)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new ListComparer<int>());

        _ = builder.HasIndex(achievement => achievement.Name);

        _ = builder.HasDiscriminator<string>("Type")
           .HasValue<Achievement>("achievement")
           .HasValue<CollectionAchievement>("collection_achievement");

#pragma warning disable CS0618 // Type or member is obsolete
        _ = builder.Ignore(achievement => achievement.IconHref);
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
