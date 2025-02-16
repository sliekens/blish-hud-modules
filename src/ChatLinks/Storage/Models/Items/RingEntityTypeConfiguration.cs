using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Items;

public sealed class RingEntityTypeConfiguration(
    ValueComparer<IReadOnlyCollection<InfusionSlotUpgradeSource>> upgradeSourceComparer,
    ValueComparer<IReadOnlyCollection<InfusionSlotUpgradePath>> upgradePathComparer
) : IEntityTypeConfiguration<Ring>
{
    public void Configure(EntityTypeBuilder<Ring> builder)
    {
        builder.Property(ring => ring.UpgradesFrom)
            .HasColumnName("UpgradesFrom")
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(upgradeSourceComparer);
        builder.Property(ring => ring.UpgradesInto)
            .HasColumnName("UpgradesInto")
            .HasJsonValueConversion()
            .Metadata.SetValueComparer((upgradePathComparer));
    }
}