using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Items;

public sealed class GenericConsumableEntityTypeConfiguration : IEntityTypeConfiguration<GenericConsumable>
{
    public void Configure(EntityTypeBuilder<GenericConsumable> builder)
    {
        builder.Property(genericConsumable => genericConsumable.Effect).HasColumnName("Effect")
            .HasJsonValueConversion();
        builder.Property(genericConsumable => genericConsumable.GuildUpgradeId)
            .HasColumnName("GuildUpgradeId");
    }
}