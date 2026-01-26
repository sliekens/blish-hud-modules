using GuildWars2.Hero.Equipment.Skiffs;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Hero.Equipment.Skiffs;

public sealed class SkiffSkinEntityTypeConfiguration : IEntityTypeConfiguration<SkiffSkin>
{
    public void Configure(EntityTypeBuilder<SkiffSkin> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.ToTable("SkiffSkins");
        _ = builder.HasKey(skiffSkin => skiffSkin.Id);
        _ = builder.HasIndex(skiffSkin => skiffSkin.Name);

        _ = builder.Property(skiffSkin => skiffSkin.DyeSlots)
            .HasJsonValueConversion();
    }
}
