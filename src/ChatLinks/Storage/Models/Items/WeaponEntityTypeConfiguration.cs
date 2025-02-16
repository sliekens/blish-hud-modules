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

public sealed class WeaponEntityTypeConfiguration(
    ValueConverter<IDictionary<Extensible<AttributeName>, int>, string> attributesConverter,
    ValueComparer<IDictionary<Extensible<AttributeName>, int>> attributesComparer,
    ValueComparer<Buff> buffComparer,
    ValueComparer<IReadOnlyList<InfusionSlot>> infusionSlotsComparer
) : IEntityTypeConfiguration<Weapon>
{
    public void Configure(EntityTypeBuilder<Weapon> builder)
    {
        builder.Property(weapon => weapon.DamageType).HasConversion(new ExtensibleEnumConverter<DamageType>());
        builder.Property(weapon => weapon.Defense).HasColumnName("Defense");
        builder.Property(weapon => weapon.DefaultSkinId).HasColumnName("DefaultSkinId");
        builder.Property(weapon => weapon.SuffixItemId).HasColumnName("SuffixItemId");
        builder.Property(weapon => weapon.SecondarySuffixItemId).HasColumnName("SecondarySuffixItemId");
        builder.Property(weapon => weapon.AttributeCombinationId).HasColumnName("AttributeCombinationId");
        builder.Property(weapon => weapon.Attributes)
            .HasColumnName("Attributes")
            .HasConversion(attributesConverter)
            .Metadata.SetValueComparer(attributesComparer);
        builder.Property(weapon => weapon.AttributeAdjustment).HasColumnName("AttributeAdjustment");
        builder.Property(weapon => weapon.StatChoices)
            .HasColumnName("StatChoices")
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new ListComparer<int>());
        builder.Property(weapon => weapon.InfusionSlots).HasColumnName("InfusionSlots")
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(infusionSlotsComparer);
        builder.Property(weapon => weapon.Buff)
            .HasColumnName("Buff")
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(buffComparer);
    }
}
