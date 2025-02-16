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

public sealed class ArmorEntityTypeConfiguration(
    ValueConverter<IDictionary<Extensible<AttributeName>, int>, string> attributesConverter,
    ValueComparer<IDictionary<Extensible<AttributeName>, int>> attributesComparer,
    ValueComparer<Buff> buffComparer,
    ValueComparer<IReadOnlyList<InfusionSlot>> infusionSlotsComparer
) : IEntityTypeConfiguration<Armor>
{
    public void Configure(EntityTypeBuilder<Armor> builder)
    {
        builder.Property(armor => armor.WeightClass).HasConversion(new ExtensibleEnumConverter<WeightClass>());
        builder.Property(armor => armor.Defense).HasColumnName("Defense");
        builder.Property(armor => armor.DefaultSkinId).HasColumnName("DefaultSkinId");
        builder.Property(armor => armor.SuffixItemId).HasColumnName("SuffixItemId");
        builder.Property(armor => armor.AttributeCombinationId).HasColumnName("AttributeCombinationId");
        builder.Property(armor => armor.Attributes)
            .HasColumnName("Attributes")
            .HasConversion(attributesConverter)
            .Metadata
            .SetValueComparer(attributesComparer);
        builder.Property(armor => armor.AttributeAdjustment).HasColumnName("AttributeAdjustment");
        builder.Property(armor => armor.StatChoices)
            .HasColumnName("StatChoices")
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new ListComparer<int>());
        builder.Property(armor => armor.InfusionSlots).HasColumnName("InfusionSlots")
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(infusionSlotsComparer);
        builder.Property(armor => armor.Buff)
            .HasColumnName("Buff")
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(buffComparer);
    }
}
