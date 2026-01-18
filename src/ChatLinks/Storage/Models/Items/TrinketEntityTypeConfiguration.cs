using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Items;

public sealed class TrinketEntityTypeConfiguration : IEntityTypeConfiguration<Trinket>
{
    public void Configure(EntityTypeBuilder<Trinket> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.Property(trinket => trinket.SuffixItemId).HasColumnName("SuffixItemId");
        _ = builder.Property(trinket => trinket.AttributeCombinationId).HasColumnName("AttributeCombinationId");
        _ = builder.Property(trinket => trinket.Attributes)
            .HasColumnName("Attributes")
            .HasImmutableValueDictionaryConverter(
                key => new Extensible<AttributeName>(key),
                ext => ext.ToString()
            );
        _ = builder.Property(trinket => trinket.AttributeAdjustment).HasColumnName("AttributeAdjustment");
        _ = builder.Property(trinket => trinket.StatChoices)
            .HasColumnName("StatChoices")
            .HasJsonValueConversion();
        _ = builder.Property(trinket => trinket.InfusionSlots).HasColumnName("InfusionSlots")
            .HasJsonValueConversion();
        _ = builder.Property(trinket => trinket.Buff)
            .HasColumnName("Buff")
            .HasJsonValueConversion();
    }
}
