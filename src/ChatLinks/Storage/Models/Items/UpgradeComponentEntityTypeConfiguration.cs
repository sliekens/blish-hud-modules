using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Items;

public class UpgradeComponentEntityTypeConfiguration : IEntityTypeConfiguration<UpgradeComponent>
{
    public void Configure(EntityTypeBuilder<UpgradeComponent> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.Property(upgradeComponent => upgradeComponent.AttributeCombinationId)
            .HasColumnName("AttributeCombinationId");
        _ = builder.Property(upgradeComponent => upgradeComponent.Attributes)
            .HasColumnName("Attributes")
            .HasImmutableValueDictionaryConverter(
                key => new Extensible<AttributeName>(key),
                ext => ext.ToString()
            );
        _ = builder.Property(upgradeComponent => upgradeComponent.AttributeAdjustment)
            .HasColumnName("AttributeAdjustment");
        _ = builder.Property(upgradeComponent => upgradeComponent.UpgradeComponentFlags)
            .HasConversion(new JsonValueConverter<UpgradeComponentFlags>());
        _ = builder.Property(upgradeComponent => upgradeComponent.InfusionUpgradeFlags)
            .HasConversion(new JsonValueConverter<InfusionSlotFlags>());
        _ = builder.Property(upgradeComponent => upgradeComponent.Buff)
            .HasColumnName("Buff")
            .HasJsonValueConversion();
        _ = builder.Property(upgradeComponent => upgradeComponent.UpgradesInto)
            .HasColumnName("UpgradesInto")
            .HasJsonValueConversion();
    }
}
