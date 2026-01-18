using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Items;

public sealed class ArmorEntityTypeConfiguration : IEntityTypeConfiguration<Armor>
{
    public void Configure(EntityTypeBuilder<Armor> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.Property(armor => armor.WeightClass).HasConversion(new ExtensibleEnumConverter<WeightClass>());
        _ = builder.Property(armor => armor.Defense).HasColumnName("Defense");
        _ = builder.Property(armor => armor.DefaultSkinId).HasColumnName("DefaultSkinId");
        _ = builder.Property(armor => armor.SuffixItemId).HasColumnName("SuffixItemId");
        _ = builder.Property(armor => armor.AttributeCombinationId).HasColumnName("AttributeCombinationId");
        _ = builder.Property(armor => armor.Attributes)
            .HasColumnName("Attributes")
            .HasImmutableValueDictionaryConverter(
                key => new Extensible<AttributeName>(key),
                ext => ext.ToString()
            );
        _ = builder.Property(armor => armor.AttributeAdjustment).HasColumnName("AttributeAdjustment");
        _ = builder.Property(armor => armor.StatChoices)
            .HasColumnName("StatChoices")
            .HasJsonValueConversion();
        _ = builder.Property(armor => armor.InfusionSlots).HasColumnName("InfusionSlots")
            .HasJsonValueConversion();
        _ = builder.Property(armor => armor.Buff)
            .HasColumnName("Buff")
            .HasJsonValueConversion();
    }
}
