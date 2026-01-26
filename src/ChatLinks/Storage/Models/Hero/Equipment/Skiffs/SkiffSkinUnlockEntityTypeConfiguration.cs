using GuildWars2.Hero.Equipment.Skiffs;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SL.ChatLinks.Storage.Models.Hero.Equipment.Skiffs;

public sealed class SkiffSkinUnlockEntityTypeConfiguration : IEntityTypeConfiguration<SkiffSkinUnlock>
{
    public void Configure(EntityTypeBuilder<SkiffSkinUnlock> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.ToTable("SkiffSkinUnlocks");
        _ = builder.HasKey(skiffSkinUnlock => new { skiffSkinUnlock.SkiffSkinId, skiffSkinUnlock.ItemId });

        _ = builder.HasOne<SkiffSkin>()
            .WithMany()
            .HasForeignKey(skiffSkinUnlock => skiffSkinUnlock.SkiffSkinId);

        _ = builder.HasOne<Item>()
            .WithMany()
            .HasForeignKey(skiffSkinUnlock => skiffSkinUnlock.ItemId);
    }
}
