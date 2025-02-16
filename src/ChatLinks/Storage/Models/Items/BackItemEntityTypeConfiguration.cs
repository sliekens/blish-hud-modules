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

public sealed class BackItemEntityTypeConfiguration(
    ValueConverter<IDictionary<Extensible<AttributeName>, int>, string> attributesConverter,
    ValueComparer<IDictionary<Extensible<AttributeName>, int>> attributesComparer,
    ValueComparer<Buff>
        buffComparer,
    ValueComparer<IReadOnlyList<InfusionSlot>> infusionSlotsComparer,
    ValueComparer<IReadOnlyCollection<InfusionSlotUpgradeSource>> upgradeSourceComparer,
    ValueComparer<IReadOnlyCollection<InfusionSlotUpgradePath>> upgradePathComparer
) : IEntityTypeConfiguration<Backpack>
{
    public void Configure(EntityTypeBuilder<Backpack> builder)
    {
        builder.Property(backpack => backpack.DefaultSkinId).HasColumnName("DefaultSkinId");
        builder.Property(backpack => backpack.SuffixItemId).HasColumnName("SuffixItemId");
        builder.Property(backpack => backpack.AttributeCombinationId).HasColumnName("AttributeCombinationId");
        builder.Property(backpack => backpack.Attributes)
            .HasColumnName("Attributes")
            .HasConversion(attributesConverter)
            .Metadata.SetValueComparer(attributesComparer);
        builder.Property(backpack => backpack.AttributeAdjustment).HasColumnName("AttributeAdjustment");
        builder.Property(backpack => backpack.StatChoices)
            .HasColumnName("StatChoices")
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new ListComparer<int>());
        builder.Property(backpack => backpack.InfusionSlots).HasColumnName("InfusionSlots")
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(infusionSlotsComparer);
        builder.Property(backpack => backpack.Buff)
            .HasColumnName("Buff")
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(buffComparer);
        builder.Property(backpack => backpack.UpgradesFrom)
            .HasColumnName("UpgradesFrom")
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(upgradeSourceComparer);
        builder.Property(backpack => backpack.UpgradesInto)
            .HasColumnName("UpgradesInto")
            .HasJsonValueConversion()
            .Metadata.SetValueComparer((upgradePathComparer));
    }
}
