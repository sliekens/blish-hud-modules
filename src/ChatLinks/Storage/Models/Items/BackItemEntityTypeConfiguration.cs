using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Items;

public sealed class BackItemEntityTypeConfiguration : IEntityTypeConfiguration<BackItem>
{
    public void Configure(EntityTypeBuilder<BackItem> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.Property(back => back.DefaultSkinId).HasColumnName("DefaultSkinId");
        _ = builder.Property(back => back.SuffixItemId).HasColumnName("SuffixItemId");
        _ = builder.Property(back => back.AttributeCombinationId).HasColumnName("AttributeCombinationId");
        _ = builder.Property(back => back.Attributes)
            .HasColumnName("Attributes")
            .HasImmutableValueDictionaryConverter(
                key => new Extensible<AttributeName>(key),
                ext => ext.ToString()
            );
        _ = builder.Property(back => back.AttributeAdjustment).HasColumnName("AttributeAdjustment");
        _ = builder.Property(back => back.StatChoices)
            .HasColumnName("StatChoices")
            .HasJsonValueConversion();
        _ = builder.Property(back => back.InfusionSlots).HasColumnName("InfusionSlots")
            .HasJsonValueConversion();
        _ = builder.Property(back => back.Buff)
            .HasColumnName("Buff")
            .HasJsonValueConversion();
        _ = builder.Property(back => back.UpgradesFrom)
            .HasColumnName("UpgradesFrom")
            .HasJsonValueConversion();
        _ = builder.Property(back => back.UpgradesInto)
            .HasColumnName("UpgradesInto")
            .HasJsonValueConversion();
    }
}
