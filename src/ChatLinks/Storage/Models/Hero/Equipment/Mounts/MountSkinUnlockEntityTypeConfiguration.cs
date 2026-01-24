using GuildWars2.Hero.Equipment.Mounts;
using GuildWars2.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SL.ChatLinks.Storage.Models.Hero.Equipment.Mounts;

// https://github.com/gw2-api/issues/issues/146
public sealed class MountSkinUnlockEntityTypeConfiguration : IEntityTypeConfiguration<MountSkinUnlock>
{
    public void Configure(EntityTypeBuilder<MountSkinUnlock> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.ToTable("MountSkinUnlocks");
        _ = builder.HasKey(mountSkinUnlock => new { mountSkinUnlock.MountSkinId, mountSkinUnlock.ItemId });

        _ = builder.HasOne<MountSkin>()
            .WithMany()
            .HasForeignKey(mountSkinUnlock => mountSkinUnlock.MountSkinId);

        _ = builder.HasOne<Item>()
            .WithMany()
            .HasForeignKey(mountSkinUnlock => mountSkinUnlock.ItemId);
    }
}

