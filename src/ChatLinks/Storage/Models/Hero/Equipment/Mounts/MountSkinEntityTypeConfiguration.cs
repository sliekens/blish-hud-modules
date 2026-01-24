using GuildWars2.Hero.Equipment.Mounts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Hero.Equipment.Mounts;

public sealed class MountSkinEntityTypeConfiguration : IEntityTypeConfiguration<MountSkin>
{
    public void Configure(EntityTypeBuilder<MountSkin> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.ToTable("MountSkins");
        _ = builder.HasKey(mountSkin => mountSkin.Id);
        _ = builder.HasIndex(mountSkin => mountSkin.Name);

        _ = builder.Property(mountSkin => mountSkin.DyeSlots)
            .HasJsonValueConversion();

        // https://github.com/gw2-api/issues/issues/146
        //_ = builder.Property(mountSkin => mountSkin.UnlockItemIds)
        //    .HasJsonValueConversion();
    }
}
