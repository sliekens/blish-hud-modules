using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Items;

public class UpgradeComponentEntityTypeConfiguration(
    ValueConverter<IDictionary<Extensible<AttributeName>, int>, string> attributesConverter,
    ValueComparer<IDictionary<Extensible<AttributeName>, int>> attributesComparer,
    ValueComparer<Buff> buffComparer
) : IEntityTypeConfiguration<UpgradeComponent>
{
    public void Configure(EntityTypeBuilder<UpgradeComponent> builder)
    {
        _ = builder.Property(upgradeComponent => upgradeComponent.AttributeCombinationId)
            .HasColumnName("AttributeCombinationId");
        builder.Property(upgradeComponent => upgradeComponent.Attributes)
            .HasColumnName("Attributes")
            .HasConversion(attributesConverter)
            .Metadata.SetValueComparer(attributesComparer);
        _ = builder.Property(upgradeComponent => upgradeComponent.AttributeAdjustment)
            .HasColumnName("AttributeAdjustment");
        _ = builder.Property(upgradeComponent => upgradeComponent.UpgradeComponentFlags)
            .HasConversion(new JsonValueConverter<UpgradeComponentFlags>());
        _ = builder.Property(upgradeComponent => upgradeComponent.InfusionUpgradeFlags)
            .HasConversion(new JsonValueConverter<InfusionSlotFlags>());
        builder.Property(upgradeComponent => upgradeComponent.Buff)
            .HasColumnName("Buff")
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(buffComparer);
    }
}
