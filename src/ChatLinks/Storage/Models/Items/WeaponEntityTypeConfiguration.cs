using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Items;

public sealed class WeaponEntityTypeConfiguration : IEntityTypeConfiguration<Weapon>
{
    public void Configure(EntityTypeBuilder<Weapon> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.Property(weapon => weapon.DamageType).HasConversion(new ExtensibleEnumConverter<DamageType>());
        _ = builder.Property(weapon => weapon.Defense).HasColumnName("Defense");
        _ = builder.Property(weapon => weapon.DefaultSkinId).HasColumnName("DefaultSkinId");
        _ = builder.Property(weapon => weapon.SuffixItemId).HasColumnName("SuffixItemId");
        _ = builder.Property(weapon => weapon.SecondarySuffixItemId).HasColumnName("SecondarySuffixItemId");
        _ = builder.Property(weapon => weapon.AttributeCombinationId).HasColumnName("AttributeCombinationId");
        _ = builder.Property(weapon => weapon.Attributes)
            .HasColumnName("Attributes")
            .HasImmutableValueDictionaryConverter(
                key => new Extensible<AttributeName>(key),
                ext => ext.ToString()
            );
        _ = builder.Property(weapon => weapon.AttributeAdjustment).HasColumnName("AttributeAdjustment");
        _ = builder.Property(weapon => weapon.StatChoices)
            .HasColumnName("StatChoices")
            .HasJsonValueConversion();
        _ = builder.Property(weapon => weapon.InfusionSlots).HasColumnName("InfusionSlots")
            .HasJsonValueConversion();
        _ = builder.Property(weapon => weapon.Buff)
            .HasColumnName("Buff")
            .HasJsonValueConversion();
    }
}
