using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Items;

public sealed class RingEntityTypeConfiguration : IEntityTypeConfiguration<Ring>
{
    public void Configure(EntityTypeBuilder<Ring> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.Property(ring => ring.UpgradesFrom)
            .HasColumnName("UpgradesFrom")
            .HasJsonValueConversion();
        _ = builder.Property(ring => ring.UpgradesInto)
            .HasColumnName("UpgradesInto")
            .HasJsonValueConversion();
    }
}
