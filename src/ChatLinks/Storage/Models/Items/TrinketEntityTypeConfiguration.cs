using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Items;

public sealed class TrinketEntityTypeConfiguration(
    ValueConverter<IDictionary<Extensible<AttributeName>, int>, string> attributesConverter,
    ValueComparer<IDictionary<Extensible<AttributeName>, int>> attributesComparer,
    ValueComparer<Buff> buffComparer,
    ValueComparer<IReadOnlyList<InfusionSlot>> infusionSlotsComparer
) : IEntityTypeConfiguration<Trinket>
{
    public void Configure(EntityTypeBuilder<Trinket> builder)
    {
        _ = builder.Property(trinket => trinket.SuffixItemId).HasColumnName("SuffixItemId");
        _ = builder.Property(trinket => trinket.AttributeCombinationId).HasColumnName("AttributeCombinationId");
        builder.Property(trinket => trinket.Attributes)
            .HasColumnName("Attributes")
            .HasConversion(attributesConverter)
            .Metadata.SetValueComparer(attributesComparer);
        _ = builder.Property(trinket => trinket.AttributeAdjustment).HasColumnName("AttributeAdjustment");
        builder.Property(trinket => trinket.StatChoices)
            .HasColumnName("StatChoices")
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new ListComparer<int>());
        builder.Property(trinket => trinket.InfusionSlots).HasColumnName("InfusionSlots")
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(infusionSlotsComparer);
        builder.Property(trinket => trinket.Buff)
            .HasColumnName("Buff")
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(buffComparer);
    }
}
